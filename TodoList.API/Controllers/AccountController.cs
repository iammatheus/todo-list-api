﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TodoList.API.Extensions;
using TodoList.Application.Contratos;
using TodoList.Application.Dtos;

namespace TodoList.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ITokenService _tokenService;

        public AccountController(IAccountService accountService, ITokenService tokenService )
        {
            _accountService = accountService;
            _tokenService = tokenService;
        }

        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var userName= User.GetUserName();
                var user = await _accountService.GetUserByUserNameAsync(userName);

                return Ok(user);
            }
            catch (Exception ex)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao tentar recuperar usuário. Erro: {ex.Message}");
            }
        }


        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            try
            {
                if (await _accountService.UserExists(userDto.UserName))
                    return BadRequest("Usuário já existe!");

                var user = await _accountService.CreateAccountAsync(userDto);

                if (user != null)
                    return Ok(new
                    {
                        userName = user.UserName,
                        fullName = user.FullName,
                        token = _tokenService.CreateToken(user).Result

                    });

                return BadRequest("Usuário não criado, tente novamente mais tarde!");

            }
            catch (Exception ex)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao tentar registrar usuário. Erro: {ex.Message}");
            }


        }


        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto userLogin)
        {
            try
            {
                   var user = await _accountService.GetUserByUserNameAsync(userLogin.UserName);
                   if(user==null) return Unauthorized("Usuário ou senha inválidos.");

                var result = await _accountService.CheckUserPasswordAsync(user, userLogin.Password);
                if(!result.Succeeded) return Unauthorized("Usuário ou senha inválidos.");

                return Ok(new 
                {
                  userName = user.UserName,
                  fullName = user.FullName,
                  token = _tokenService.CreateToken(user).Result

                });
            }
            catch (Exception ex)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao tentar efetuar login. Erro: {ex.Message}");
            }


        }


        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(UserUpdateDto updateUser)
        {
            try
            {
                if(updateUser.UserName != User.GetUserName()) return Unauthorized("Usuário inválido!");
                var user = await _accountService.GetUserByUserNameAsync(User.GetUserName());
                if(user==null) return Unauthorized("Usuário inválido!");

                var userReturn = await _accountService.UpdateAccount(updateUser);
                if (userReturn == null) return NoContent();

                return Ok(new
                {
                    userName = user.UserName,
                    fullName = user.FullName,
                    token = _tokenService.CreateToken(user).Result
                });

            }
            catch (Exception ex)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao tentar atualizar usuário. Erro: {ex.Message}");
            }


        }


    }
}
