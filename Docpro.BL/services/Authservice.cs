using Docpro.BL.Dtos;
using Docpro.BL.Helper;
using Docpro.DAL.Database;
using Docpro.DAL.extend;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
 
namespace Docpro.BL.services
{
    public class Authservice : IAuthservice
    {
        #region fields
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IOptions<JWT> jwt;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly AppDbContext context;

        #endregion

        #region Ctor
        public Authservice(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            this.userManager = userManager;
            this.jwt = jwt;
            this.roleManager = roleManager;
            this.context = context;
        } 
        #endregion

        #region Login
        public async Task<AuthModel> Login(LoginDTO loginDTO)
        {
            var user = await userManager.FindByEmailAsync(loginDTO.Email);

            if (user == null || !await userManager.CheckPasswordAsync(user, loginDTO.Password))
                return new AuthModel { Message = "Inavlid Email Or Password" };

            //if (user.EmailConfirmed == false)
            //{
            //    return new AuthModel { Message = "Please Check Your Inbox To Confirm Email" };
            //}

            await context.SaveChangesAsync();

            var jwtSecurityToken = await CreateJwtToken(user);

            return new AuthModel
            {
                Message = "Success",
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthencated = true
            };
        }
        #endregion

        #region Register
 
        public async Task<AuthModel> Register(RegisterDTO registerDTO)
        {
            try
            {
                if (await userManager.FindByEmailAsync(registerDTO.Email) != null)
                    return new AuthModel { Message = "Email Is Already Token" };
                var user = new ApplicationUser
                {
                    Email = registerDTO.Email,
                    UserName = registerDTO.Email,
                    FirstName = registerDTO.FirstName,
                    LastName = registerDTO.LastName,
                    PhotoName = "defualt.png",
                    Status = "Patient"
                };
                var result = await userManager.CreateAsync(user, registerDTO.Password);

                if (!result.Succeeded)
                {
                    var error = string.Empty;
                    foreach (var item in result.Errors)
                    {
                        error += $"{item.Description},";
                    }
                    return new AuthModel { Message = error };
                }

                var RoleExsit = await roleManager.RoleExistsAsync("Admin");
                if (!RoleExsit)
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                    await userManager.AddToRoleAsync(user, "Admin");
                    user.Status = "Admin";
                    await userManager.UpdateAsync(user);
                }
                else
                {
                    await roleManager.CreateAsync(new IdentityRole("Patient"));
                    await userManager.AddToRoleAsync(user, "Patient");
                }

                var jwtSecurityToken = await CreateJwtToken(user);


                return new AuthModel
                {
                    Message = "Success",
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                    ExpiresOn = jwtSecurityToken.ValidTo,
                    IsAuthencated = true
                };
            }
            catch (Exception)
            {
                throw;
            }

        }

        #endregion

        #region Create Token
        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await userManager.GetClaimsAsync(user);
            var roles = await userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("photoName", user.PhotoName),
                new Claim("fullName", user.FullName),
                new Claim("status", user.Status),
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Value.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: jwt.Value.Issuer,
                audience: jwt.Value.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(jwt.Value.DurationInDays),
                signingCredentials: signingCredentials);



            return jwtSecurityToken;


        }
        #endregion
    }
}
