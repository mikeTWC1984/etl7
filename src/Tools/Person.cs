using System;
using System.Globalization;


namespace ETL.Tools
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int Age { get => (DateTime.Now - DateOfBirth).Days / 365; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int ZipCode { get; set; }
        public string PhoneNumber { get; set; }
        public int Account { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public DateTime LastUpdate { get; set; }
        public int StrNumber { get; set; }

        public Person(Random random)
        {
            Id = random.Next(100000000, 190000000);

            Gender = random.Next(2) > 0 ? "M" : "F";
            FirstName = Gender == "M" ? 
                PersonData.MaleFirstNames[random.Next(PersonData.MaleFirstNames.Length)] : 
                PersonData.FemailFirstNames[random.Next(PersonData.FemailFirstNames.Length)];
            MiddleName = ((char)(random.Next(65, 91))).ToString();
            LastName = CultureInfo.GetCultureInfo(CultureInfo.CurrentCulture.Name).TextInfo.ToTitleCase(
                PersonData.LastNames[random.Next(PersonData.LastNames.Length)].ToLowerInvariant());

            DateOfBirth = DateTime.Now.AddDays(-random.Next(PersonData.MINIMUM_AGE * 365, PersonData.MAXIMUM_AGE * 365));

            Street = PersonData.Streets[random.Next(PersonData.Streets.Length)];
            City = PersonData.Cities[random.Next(PersonData.Cities.Length)];
            State = PersonData.States[random.Next(PersonData.States.Length)];
            ZipCode = random.Next(10000, 90000);
            PhoneNumber = (3470000000L + random.NextLong(5000000000L)).ToString("000-000-0000");

            Account = random.Next(1000000, 2000000);
            Rate = (decimal)random.Next(1000) / 1017;
            Amount = Math.Round(((decimal)200 / (10 + random.Next(17))), 2);
            Balance = Math.Round(((decimal)200 / (10 + random.Next(17))), 4);
            LastUpdate = DateTime.Now.AddSeconds(-random.Next(10000));

            StrNumber = 1 + random.Next(10000);
        }

        public override string ToString() =>
            $"Id={Id}\n" +
            $"Name: {FirstName} {MiddleName}. {LastName}\n" +
            $"Date of Birth: {DateOfBirth:yyyy-MM-dd} ({Age})\tGender: {Gender}\n\n" +
            $"Address: {Street}, {City}, {State} {ZipCode}\n" +
            $"Phone number: {PhoneNumber}\n\n" +
            $"Account: {Account}\tRate: {Rate}\n" +
            $"Amount: {Amount}\tBalance: {Balance}\n" +
            $"LastUpdated: {LastUpdate:f}\n\n" +
            $"StrNumber: {StrNumber}";
    }
}
