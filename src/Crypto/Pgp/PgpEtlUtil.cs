
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Bcpg;
using ETL.File.InputTypes;

namespace ETL.Crypto.Pgp
{
    public static class PgpEtlUtil
    {
        public static Boolean Armor { get; set; } = true;
        public static SymmetricKeyAlgorithmTag SymKeyAlg { get; set; } = SymmetricKeyAlgorithmTag.Aes256;
        public static CompressionAlgorithmTag CompressionAlg { get; set; } = CompressionAlgorithmTag.Zip;
        public static Int32 BufferSize { get; set; } = (Int32)Math.Pow(2, 15);

        public static T GetPgpKeyRing<T>(StreamData keyData) where T : class
        {
            using (var kp = PgpUtilities.GetDecoderStream(keyData.Stream))
            {
                var limit = 3; // scan up to 3 keys in the file
                while (limit > 0)
                {
                    var keyObjects = new PgpObjectFactory(kp);
                    var ring = keyObjects.NextPgpObject();
                    if (ring is null) break;
                    if (ring is T) return ring as T;
                    limit--;
                }              
                
            }
             throw new Exception("Keyfile does not contain: " + typeof(T).ToString());
             //return null;

        }

        /// <summary>
        /// Scan key file for public keys. File may contain multiple keys (public or private)
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        public static List<PgpPublicKey> SearchPgpPublicKeys(StreamData keyData)
        {
            using (var kp = PgpUtilities.GetDecoderStream(keyData.Stream))
            {
                var keys = new List<PgpPublicKey>();
                var limit = 3; // scan up to 3 keys in the file
                while (limit > 0)
                {
                    var keyObjects = new PgpObjectFactory(kp);
                    var ring = keyObjects.NextPgpObject();
                    if (ring is null) break;
                    if (ring is PgpPublicKeyRing)
                    {
                        foreach(PgpPublicKey key in ((PgpPublicKeyRing)ring).GetPublicKeys())
                        {
                            keys.Add(key);
                        }
                    }
                     if (ring is PgpSecretKeyRing)
                    {
                        var key = ((PgpSecretKeyRing)ring).GetPublicKey();
                        keys.Add(key);
                    }
                    limit--;
                }

                return keys;

                //throw new Exception("Keyfile does not contain: " + typeof(T).ToString());

            }

        }

        ///<summary>
        /// Will check all public keys (master and subkeys) with IsEncryptionKey flag = true, but returns first available subkey (then master then error)
        ///</summary>
        public static PgpPublicKey GetPgpPublicKey(StreamData keyData)
        {
            // var keyRing = GetPgpKeyRing<PgpPublicKeyRing>(keyData);

            // if(keyRing is null) // if null we are likely reading secret key file
            // {
            //     var skr = GetPgpKeyRing<PgpSecretKeyRing>(keyData);
            //     if(skr is null) throw new Exception("Cannot locate public key");
            //     Console.WriteLine(skr.GetPublicKey());
            //     return skr.GetPublicKey();
            // }

            var keys = SearchPgpPublicKeys(keyData);

            PgpPublicKey masterKey = null;

            foreach (PgpPublicKey key in keys) // keyRing.GetPublicKeys()
            {
                if (key.IsEncryptionKey)
                {
                    if (key.IsMasterKey) { masterKey = key; } else { return key; }
                }
            }

            return masterKey ?? throw new Exception("There is no encryption keys on this key file");
        }


        public static List<PgpPublicKey> GetPgpPublicKeys(StreamData keyData)
        {
            var keyList = new List<PgpPublicKey>();
            var keyRing = GetPgpKeyRing<PgpPublicKeyRing>(keyData);
            foreach (PgpPublicKey key in keyRing.GetPublicKeys())
            {
                keyList.Add(key);
            }
            return keyList;
        }

        ///<summary>
        /// Will check all private keys (master and subkeys), but returns first available subkey (or then sign key then master and then error)
        ///</summary>
        public static PgpPrivateKey GetPgpPrivateKey(StreamData keyData, String passphrase)
        {
            var keyRing = GetPgpKeyRing<PgpSecretKeyRing>(keyData);

            PgpPrivateKey masterKey = null;
            PgpPrivateKey signKey = null;

            foreach (PgpSecretKey key in keyRing.GetSecretKeys())
            {
                if (key.IsMasterKey) { masterKey = key.ExtractPrivateKey(passphrase.ToCharArray()); }
                else if (!key.IsMasterKey & key.IsSigningKey) { signKey = key.ExtractPrivateKey(passphrase.ToCharArray()); }
                else { return key.ExtractPrivateKey(passphrase.ToCharArray()); }
            }

            return signKey ?? masterKey ?? throw new Exception("There is no private key on this key file");
        }

        public static PgpPrivateKey GetPgpPrivateKeyById(StreamData keyData, String passphrase, long id)
        {
            var keyRing = GetPgpKeyRing<PgpSecretKeyRing>(keyData);

            var privateKey = keyRing.GetSecretKey(id);

            // PgpPrivateKey privateKey = null;

            // foreach (PgpSecretKey key in keyRing.GetSecretKeys())
            // {
            //     if (key.KeyId == id) { return key.ExtractPrivateKey(passphrase.ToCharArray()); }
            // }

            if(privateKey != null)  return privateKey.ExtractPrivateKey(passphrase.ToCharArray());

            throw new Exception("Can't locate private key with id" + id);

        }

        public static List<PgpPrivateKey> GetPgpPrivateKeys(StreamData keyData, String passphrase)
        {
            var keyList = new List<PgpPrivateKey>();
            var keyRing = GetPgpKeyRing<PgpSecretKeyRing>(keyData);

            foreach (PgpSecretKey key in keyRing.GetSecretKeys())
            {
                keyList.Add(key.ExtractPrivateKey(passphrase.ToCharArray()));
            }
            return keyList;
        }

        /// <summary>
        ///  Secret key is unextracted "Private" key
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static List<PgpSecretKey> GetPgpSecretKeys(StreamData keyData)
        {
            var keyList = new List<PgpSecretKey>();
            var keyObjects = new PgpObjectFactory(PgpUtilities.GetDecoderStream(keyData.Stream));
            var keyRing = keyObjects.NextPgpObject() as PgpSecretKeyRing ?? throw new Exception("File does not contain SecretKeyRing");

            foreach (PgpSecretKey key in keyRing.GetSecretKeys())
            {
                keyList.Add(key);
            }
            return keyList;
        }

        public static PgpWriter GetPgpWriter(Stream outStream, StreamData keyData)
        {
            return new PgpWriter(outStream, keyData, BufferSize, SymKeyAlg, CompressionAlg, Armor);
        }

        public static void EncryptStream(Stream inStream, Stream outStream, StreamData keyData
               , Boolean armor = false
               , int bufferSize = 32768
               , CompressionAlgorithmTag compAlg = CompressionAlgorithmTag.Zip
               , SymmetricKeyAlgorithmTag symKeyAlg = SymmetricKeyAlgorithmTag.Aes256
               )
        {
            if (!outStream.CanWrite) { throw new Exception("Input stream must be writeable"); }
            var key = GetPgpPublicKey(keyData);
            var encryptor = new PgpEncryptedDataGenerator(symKeyAlg, true);
            encryptor.AddMethod(key);

            using (inStream)
            using (outStream)
            using (var outData = armor ? new ArmoredOutputStream(outStream) : outStream)
            {
                using (var encryptedData = encryptor.Open(outData, new Byte[bufferSize]))
                {
                    using (var compressedData = (new PgpCompressedDataGenerator(compAlg)).Open(encryptedData, new Byte[bufferSize]))
                    {
                        using (var literalData = (new PgpLiteralDataGenerator()).Open(compressedData, PgpLiteralData.Binary, "", DateTime.Now, new Byte[bufferSize]))
                        {
                            Org.BouncyCastle.Utilities.IO.Streams.PipeAll(inStream, literalData);
                        }
                    }
                }
            }
        }

        public static void DecryptStream(Stream inStream, Stream outStream, StreamData keyData, String passPhrase)
        {
            using (var newStream = PgpUtilities.GetDecoderStream(inStream))
            {
                PgpObjectFactory pgpObjF = new PgpObjectFactory(newStream);
                PgpEncryptedDataList enc;
                PgpObject obj = pgpObjF.NextPgpObject();
                if (obj is PgpEncryptedDataList)
                {
                    enc = (PgpEncryptedDataList)obj;
                    
                }
                else
                {
                    enc = (PgpEncryptedDataList)pgpObjF.NextPgpObject();
                }

                PgpPublicKeyEncryptedData pbe = enc.GetEncryptedDataObjects().Cast<PgpPublicKeyEncryptedData>().First();
                PgpPrivateKey privKey = GetPgpPrivateKeyById(keyData, passPhrase, pbe.KeyId);

                using (Stream clear = pbe.GetDataStream(privKey))
                {
                    PgpObjectFactory plainFact = new PgpObjectFactory(clear);
                    PgpObject message = plainFact.NextPgpObject();

                    if (message is PgpCompressedData)
                    {
                        PgpCompressedData cData = (PgpCompressedData)message;
                        Stream compDataIn = cData.GetDataStream();
                        PgpObjectFactory o = new PgpObjectFactory(compDataIn);
                        message = o.NextPgpObject();
                        if (message is PgpOnePassSignatureList)
                        {
                            message = o.NextPgpObject();
                        }
                        PgpLiteralData Ld = null;
                        Ld = (PgpLiteralData)message;
                        using (outStream)
                        {
                            Org.BouncyCastle.Utilities.IO.Streams.PipeAll(Ld.GetInputStream(), outStream);
                        }
                    }
                }
            }

        }

        public static PgpReader GetPgpReader(StreamData inStream, StreamData keyData, String passPhrase)
        {
            return new PgpReader(inStream.Stream, keyData, passPhrase);
        }

    }
}