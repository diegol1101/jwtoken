using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Dtos;
using API.Helpers;
using Dominio.Entities;
using Dominio.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;



namespace API.Services;

    public class UserService: IUserService
    {
        private readonly JWT _jwt;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<User> _passwordHasher;
        public UserService(IUnitOfWork unitOfWork,IOptions<JWT>jwt, IPasswordHasher<User> passwordHasher)
        {
            _jwt = jwt.Value;
            _unitOfWork = unitOfWork;
            _passwordHasher= passwordHasher;
        }

        public async Task<string> RegisterAsync(RegisterDto registerDto)
        {
            var usuario = new User
            {
                Email=registerDto.Email,
                UserName=registerDto.UserName,
            };

            usuario.Pasword = _passwordHasher.HashPassword(usuario, registerDto.Password);
            var usuarioExiste = _unitOfWork.Users
                                                .Find(u => u.UserName.ToLower()== registerDto.UserName.ToLower())
                                                .FirstOrDefault();
            if(usuarioExiste == null)
            {
                try
                {
                    _unitOfWork.Users.Add(usuario);
                    await _unitOfWork.SaveAsync();
                    
                    return $"El Usuario {registerDto.UserName} ha sido regitrado exitosamente";
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                    return $"Error:{message}";
                }
            }
            else
            {
                return $"El usuario con{registerDto.UserName} ya se encuetra registrado";
            }
        }

        public async Task<string> AddRoleAsync(AddRoleDto model)
        {
            var usuario = await _unitOfWork.Users
                                .GetByUsernameAsync(model.Username);
            if (usuario ==null)
            {
                return $"No existe algun usuario registrado con la cuenta, olvido algun caracter ? {model.Username}";
            }
            var resultado= _passwordHasher.VerifyHashedPassword(usuario,usuario.Pasword, model.Password);

            if (resultado == PasswordVerificationResult.Success)
            {
                var rolExiste = _unitOfWork.Rols
                                                .Find(u=> u.Nombre.ToLower()==model.Rol.ToLower())
                                                .FirstOrDefault();
                if (rolExiste !=null)
                {
                    var usuarioTieneRol = usuario.Rols
                                            .Any(u=> u.Id ==rolExiste.Id);
                    if (usuarioTieneRol == false)
                    {
                        usuario.Rols.Add(rolExiste);
                        _unitOfWork.Users.Update(usuario);
                        await _unitOfWork.SaveAsync();
                    }
                    return $"Rol {model.Rol} agregado a la cuenta {model.Username} de forma exitosa.";
                }
                return $"Rol{model.Rol} no encontrado";
            }
            return $"credenciales incorrectas para el usuario{usuario.UserName}";
        }
        public async Task<DatosUsuarioDto> GetTokenAsync(LoginDto model)
        {
            DatosUsuarioDto datosUserDto = new DatosUsuarioDto();
            var User = await _unitOfWork.Users
                            .GetByUsernameAsync(model.UserName);

            if (User == null)
            {
                datosUserDto.EstaAutenticado = false;
                datosUserDto.Mensaje = $"No existe ningun User con el username {model.UserName}.";
                return datosUserDto;
            }

            var result = _passwordHasher.VerifyHashedPassword(User, User.Pasword, model.Password);
            if (result == PasswordVerificationResult.Success)
            {
                datosUserDto.Mensaje = "OK";
                datosUserDto.EstaAutenticado = true;
                if (User != null)
                {
                    JwtSecurityToken jwtSecurityToken = CreateJwtToken(User);
                    datosUserDto.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                    datosUserDto.UserName = User.UserName;
                    datosUserDto.Email = User.Email;
                    datosUserDto.Roles = User.Rols
                                                        .Select(p => p.Nombre)
                                                        .ToList();


                    return datosUserDto;
                }
                else
                {
                    datosUserDto.EstaAutenticado = false;
                    datosUserDto.Mensaje = $"Credenciales incorrectas para el User {User.UserName}.";

                    return datosUserDto;
                }
            }

            datosUserDto.EstaAutenticado = false;
            datosUserDto.Mensaje = $"Credenciales incorrectas para el User {User.UserName}.";
            return datosUserDto;

        }
        private JwtSecurityToken CreateJwtToken(User User)
        {
            if (User == null)
            {
                throw new ArgumentNullException(nameof(User), "El User no puede ser nulo.");
            }

            var Rols = User.Rols;
            var roleClaims = new List<Claim>();
            foreach (var role in Rols)
            {
                roleClaims.Add(new Claim("roles", role.Nombre));
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, User.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("uid", User.Id.ToString())
            }
            .Union(roleClaims);

            if (string.IsNullOrEmpty(_jwt.Key) || string.IsNullOrEmpty(_jwt.Issuer) || string.IsNullOrEmpty(_jwt.Audience))
            {
                throw new ArgumentNullException("La configuración del JWT es nula o vacía.");
            }

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);

            var JwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials);

            return JwtSecurityToken;
        }
    }
