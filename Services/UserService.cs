using Dapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Vantex.Technician.Service.Entities;
using Vantex.Technician.Service.Helpers;
using Vantex.Technician.Service.Repositories;

namespace Vantex.Technician.Service.Services
{

    public class UserService : IUserService
    {

        private readonly AppSettings _appSettings;
        private readonly UserRepository _userRepository;

        public UserService(IOptions<AppSettings> appSettings, UserRepository userRepository)
        {
            if (appSettings == null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }
            _appSettings = appSettings.Value;
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public User Authenticate(string probeId, string userPass)
        {
            var user = _userRepository.GetByProbeId(probeId, userPass).SingleOrDefault();

            if (user == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                        new Claim(ClaimTypes.Name, user.Id.ToString()),
                        new Claim("BaseId", user.BaseId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            user.Password = null;

            return user;
        }

        public int? GetBaseIdClaim(ClaimsIdentity identity)
        {
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                return Convert.ToInt32(claims.Where(c => c.Type == "BaseId").Select(x => x.Value).FirstOrDefault());
            }
            return null;
        }
    }
}
