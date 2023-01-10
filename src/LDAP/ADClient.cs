
using System;
using Novell.Directory.Ldap;
using System.Management.Automation;
using System.Collections.Generic;
using System.Linq;

namespace ETL.LDAP
{

    public class ADClient : Novell.Directory.Ldap.LdapConnection
    {
        public String Root { get; private set; } = "";
        public ADClient(string loginServer, PSCredential credential, int port = 389, int version = 3)
        {
            this.Connect(loginServer, port);
            if (credential != null)
            {
                this.Bind(version, credential.UserName, credential.GetNetworkCredential().Password);
            }
            else { this.Bind(null, null); }

            try // try to determine Root
            {
                var sr = this.Search("", 0, "objectclass=*", null, false, null, null).GetResponse() as LdapSearchResult;
                if (sr != null)
                {
                    this.Root = sr.Entry.GetAttribute("rootDomainNamingContext").StringValue;
                }

            }
            catch { }
        }

        public ADClient(string loginServer, string credentialAlias, int port = 389, int version = 3) : this(
            loginServer,
             ETL.Config.GetCredential(credentialAlias) ?? throw new Exception("Provided credential alias does not exist"),
             port,
             version
        )
        { }

        private LdapSearchResult BasicSearch(String root, String filter, String[] attr = null)
        {
            return this.Search(root, root != null ? 2 : 0, filter, attr, false, null, null).GetResponse() as LdapSearchResult;
        }

        public List<Object> GetGroupMembers(string groupName, Boolean asEntity = false, String[] att = null, int entityLimit = 100)
        {
            var result = new List<Object>();
            var filter = String.Format("(&(objectClass=group)(cn={0}))", groupName);
            var grp = this.BasicSearch(this.Root, filter);
            if (grp != null)
            {
                if (asEntity)
                {
                    foreach (var r in grp.Entry.GetAttribute("member").StringValueArray.Take(entityLimit))
                    {
                        var mbr = this.BasicSearch(r, null, att);
                        if (mbr != null) { result.Add(mbr); }
                    }
                }
                else
                {
                    foreach (var r in grp.Entry.GetAttribute("member").StringValueArray)
                    {
                        result.Add(r.Substring(0, r.IndexOf(",OU")).Replace("\\,", ","));
                    }
                }
            }

            return result;
        }

        public List<String> GetUserGroup(string userNameOrEmail)
        {
            var result = new List<String>();
            var filter = String.Format("(&(objectCategory=person)(objectClass=organizationalPerson)(|(samaccountname={0})(mail={0})))", userNameOrEmail, userNameOrEmail);
            var grp = this.Search(this.Root, this.Root != null ? 2 : 0, filter, null, false, null, null).GetResponse() as LdapSearchResult;
            if (grp != null)
            {
                foreach (var r in grp.Entry.GetAttribute("memberOf").StringValueArray)
                {
                    result.Add(r.Substring(0, r.IndexOf(",OU")).Replace("\\,", ","));
                }
            }

            return result;
        }

        public LdapEntry GetUser(string userNameOrEmail, String[] attrs = null)
        {
            LdapEntry result = null;
            var filter = String.Format("(&(objectCategory=person)(objectClass=organizationalPerson)(|(samaccountname={0})(mail={0})))", userNameOrEmail, userNameOrEmail);
            var res = this.Search(this.Root, this.Root != null ? 2 : 0, filter, attrs, false, null, null).GetResponse() as LdapSearchResult;
            if (res != null) { result = res.Entry; }
            return result;
        }

    }

}