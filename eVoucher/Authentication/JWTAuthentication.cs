using eVoucher.Model;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace eVoucher.Authentication
{
    public class JWTAuthentication : iJWTAuthentication
    {
        string user = "AABB";
        string pass = "CDE";
        private readonly string TokenKey;
        public JWTAuthentication(string tokenKey)
        {
            TokenKey = tokenKey;
        }
        public string ValidateAndCreateJWT(AuthenticationModel reqModel)
        {           
            var tokenHandler = new JwtSecurityTokenHandler();             
            return tokenHandler.WriteToken(tokenHandler.CreateToken(
                new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, reqModel.Password)
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials =
                    new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.ASCII.GetBytes(TokenKey)),
                        SecurityAlgorithms.HmacSha256Signature)
                }));
          

        }
    }
}
