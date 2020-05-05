﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Timetracker.Entities.Classes;
using Timetracker.Entities.Models;
using Timetracker.View;

namespace View.Controllers
{
    public class UserView
    {
        public string Login { get; set; }

        public string Pass { get; set; }
    }

    [Produces("application/json")]
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private readonly TimetrackerContext _dbContext;

        public AccountController(TimetrackerContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(UserView view)
        {
            var dbUser = await _dbContext.GetUserAsync(view.Login);
            if (dbUser == null)
            {
                return Unauthorized();
            }

            var password = PasswordHelpers.EncryptPassword(view.Pass, dbUser.Salt, 1024);
            if (!PasswordHelpers.SlowEquals(password, dbUser.Pass))
            {
                return Unauthorized();
            }

            var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, dbUser.Login)
                };

            var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                    issuer: TimetrackerAuthorizationOptions.ISSUER,
                    audience: TimetrackerAuthorizationOptions.AUDIENCE,
                    notBefore: now,
                    claims: claimsIdentity.Claims,
                    expires: now.Add(TimeSpan.FromDays(TimetrackerAuthorizationOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(TimetrackerAuthorizationOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                status = HttpStatusCode.OK,
                access_token = encodedJwt
            };

            return new JsonResult(response);
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserView view)
        {
            var userExist = _dbContext.UserExists(view.Login);

            if (userExist)
            {
                return BadRequest(new
                {
                    text = "Пользователь с таким именем уже существует."
                });
            }

            var salt = PasswordHelpers.GenerateSalt(16);
            var hash = PasswordHelpers.EncryptPassword(view.Pass, salt, 1024);

            var user = new User
            {
                Login = view.Login,
                Pass = hash,
                Salt = salt
            };
  
            await _dbContext.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var response = new
            {
                status = HttpStatusCode.OK,
                text = "Вы успешно зарегистрировались!"
            };

            return new JsonResult(response);
        }
    }
}
