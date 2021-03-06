﻿namespace CompanyManagementSystem.WebServices.Controllers.Abstraction
{
    using Data.Abstraction;
    using Microsoft.AspNetCore.Mvc;
    using Services.Abstraction;
    using System;
    using System.Linq;
    using DbModels;

    [Route("api/[controller]")]
    public abstract class BaseController : Controller
    {
        private readonly string key;
        private readonly IEncryptionService encryptor;
        private readonly IUnitOfWork data;

        protected BaseController(IUnitOfWork data, IEncryptionService encryptor)
        {
            this.data = data;
            this.encryptor = encryptor;
            this.key = "JS Sucks";
        }

        public IActionResult IsAuthorized()
        {
            var cookie = this.Request.Headers.FirstOrDefault(x => x.Key == "Authorization").Value;

            if (!this.ValidateCookie(cookie))
            {
                return this.BadRequest("Invalid cookie!");
            }

            this.ExtendCookie(cookie);

            return null;
        }

        public long GetCurrentUserId()
        {
            var cookie = this.Request.Headers.FirstOrDefault(x => x.Key == "Authorization").Value;
            var decrypted = this.DecryptString(cookie);
            var idIndex = decrypted.LastIndexOf("-");
            var id = decrypted.Substring(idIndex + 1);
            return long.Parse(id);
        } 

        public string EncryptString(string value)
        {
            return this.encryptor.Encrypt(value, key);
            return value;
        }

        public string DecryptString(string value)
        {
            return this.encryptor.Decrypt(value, key);
            return value;
        }

        public Cookie CreateCookie(string name, string password, long id)
        {
            var cookie = new Cookie()
            {
                Content = this.EncryptString($"{name}-{password}-{id}"),
                UserId = id
            };

            this.data.CookieRepository.Add(cookie);
            this.data.SaveChanges();

            return cookie;
        }

        public bool ValidateCookie(string value)
        {
            var cookie = this.data.CookieRepository
                .All()
                .FirstOrDefault(x => x.Content == value && x.ExpirationDate > DateTime.Now);

            if (cookie == null)
            {
                return false;
            }

            return true;
        }

        public bool ExtendCookie(string value)
        {
            var result = false;

            try
            {
                var cookie = this.data.CookieRepository
                    .All()
                    .Where(x => x.Content == value)
                    .FirstOrDefault();

                if (cookie == null)
                {
                    throw new Exception();
                }

                if (cookie.ExpirationDate < DateTime.Now.Add(new TimeSpan(1, 0, 15, 0)))
                {
                    var date = cookie.ExpirationDate;
                    cookie.ExpirationDate = date.Add(new TimeSpan(1, 0, 0, 0));
                    this.data.CookieRepository.Update(cookie);
                    this.data.SaveChanges();
                }

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }
    }
}
