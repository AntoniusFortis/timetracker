﻿using Microsoft.AspNetCore.Http;

namespace Timetracker.Entities.Responses
{
    public class SignInModel
    {
        public string Login { get; set; }

        public string Pass { get; set; }
    }

    public class AccountModel
    {
        public string Login { get; set; }

        public string Pass { get; set; }

        public string FirstName { get; set; }

        public string Surname { get; set; }

        public string MiddleName { get; set; }

        public string City { get; set; }

        public string BirthDate { get; set; }

        public string Email { get; set; }
    }

    public class MyPageModel
    {
        public string Login { get; set; }

        public string CurrentPass { get; set; }

        public string Pass { get; set; }

        public string FirstName { get; set; }

        public string Surname { get; set; }

        public string MiddleName { get; set; }

        public string City { get; set; }

        public string BirthDate { get; set; }

        public string Email { get; set; }
    }
}
