using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos.Securite;
using UniversiteEFDataProvider.Entities;

namespace UniversiteRestApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController:ControllerBase
{
    private readonly IConfiguration _configuration;
    IUniversiteUserRepository  _userRepository;
    IUniversiteRoleRepository _roleRepository;
    
    public AccountController(IRepositoryFactory repositoryFactory, IConfiguration configuration)
    {
        _userRepository = repositoryFactory.UniversiteUserRepository();
        _roleRepository = repositoryFactory.UniversiteRoleRepository();
        _configuration = configuration;
        }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        //On commence par vérifier que l'email correspond bien à un user reconnu
        UniversiteUser user = (UniversiteUser) await _userRepository.FindByEmailAsync(login.Email);
        // On vérifie que le mot de passe est correct
        if (user != null && await _userRepository.CheckPasswordAsync(user, login.Password))
        {
            // Authentification réussie
            // On récupère les infos et on construit le token Jwt
            // Liste des rôles remplis par le user
            IList<string> userRoles= await _userRepository.GetRolesAsync(user);
            // Récolte desinformations concernant le user
            List<Claim> authClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, login.Email!),
                new Claim("userId",user.Id)
            };
            authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
            //COnstruction du jeton Jwt
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                claims: authClaims,
                expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!)),
                signingCredentials:new SigningCredentials(new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)), 
            SecurityAlgorithms.HmacSha256)
                );
            // Renvoi du token au front
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
        return Unauthorized();
    }
}