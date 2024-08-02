// Controllers/msgController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.utils;
using WebApplication1.Methods;
using Azure.Core;


namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class msgController : ControllerBase
    {
        private readonly MessagingDashboardContext _context;
        private readonly IConfiguration _config;

        public msgController(MessagingDashboardContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: api/msg
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();        

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/msg/username
         [HttpGet("{UserName}")]
        public async Task<IActionResult> GetUserByUserName(string UserName)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == UserName);

                var userDetails = _context.Users
                          .Where(u => u.UserId == user.UserId) // Filter by user.UserId
                           .Join(
                               _context.UserRoleMappings,
                               u => u.UserId,
                               urm => urm.UserId,
                               (u, urm) => new { User = u, UserRoleMapping = urm }
                           ).Join(
                               _context.Roles,
                               combined => combined.UserRoleMapping.RoleId,
                               r => r.RoleId.ToString(),
                               (combined, r) => new
                               {
                                   UserId = combined.User.UserId,
                                   UserName = combined.User.UserName,
                                   FirstName = combined.User.FirstName,
                                   LastName = combined.User.LastName,
                                   IsActive = combined.User.IsActive,
                                   CreatedDate = combined.User.CreatedDate,
                                   RoleName = r.RoleName,
                                   Description = r.Description
                               }
                           )
                           .FirstOrDefaultAsync();

                var ObjUserDetails = userDetails.Result;

                return Ok(ObjUserDetails);                   


               // return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving user with username '{UserName}': {ex.Message}");
            }
        }


        // POST: api/msg/createuser
        [HttpPost("createuser")]
        public async Task<IActionResult> CreateUser(CreateUsersRequest userRequest)
        {
            try
            {
                // Validate email format
                var emailRegex = new Regex(@"^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$");
                if (!emailRegex.IsMatch(userRequest.UserName))
                {
                    return BadRequest("Invalid email format");
                }

                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userRequest.UserName);
                if (existingUser != null)
                {
                    return BadRequest("User already exists");
                }

                // Create new User object
                var newUser = new Users
                {
                    UserName = userRequest.UserName,
                    FirstName = userRequest.FirstName,
                    LastName = userRequest.LastName,
                    Password = userRequest.Password, // Store plain text password
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                // Add user to context and save changes
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Find RoleId corresponding to RoleName
                var roleId = await GetRoleId(userRequest.RoleName);
                if (roleId == null)
                {
                    return BadRequest("Role not found");
                }

                Console.WriteLine(roleId.ToString());

                // Insert into userrolemapping table
                var userRoleMapping = new UserRoleMapping { UserId = newUser.UserId, RoleId = roleId.ToString() };

                _context.UserRoleMappings.Add(userRoleMapping);
                await _context.SaveChangesAsync();

                // Prepare success response
                var response = new
                {
                    UserId = newUser.UserId,
                    Message = "User inserted successfully"
                };

                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating user: {ex}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
            }
        }



        // POST: api/msg/authdata
        [HttpPost("authdata")]
        public async Task<IActionResult> AuthData(AuthRequest request)
        {
            try
            {
                // Find user by username
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);

                if (user == null)
                {
                    var response = new { success = false, message = "User does not exist" };
                    return NotFound(response);
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    var response = new { success = false, message = "User is not active, contact admin" };
                    return Unauthorized(response);
                }

                // Compare passwords
                if (request.Password != user.Password)
                {
                    var response = new { success = false, message = "Invalid password" };
                    return Unauthorized(response);
                }

                // Fetch user details including role information using JOIN
                var userDetails = await _context.Users
                    .Where(u => u.UserId == user.UserId)
                    .Join(
                        _context.UserRoleMappings,
                        u => u.UserId,
                        urm => urm.UserId,
                        (u, urm) => new { User = u, UserRoleMapping = urm }
                    ).Join(
                        _context.Roles,
                        combined => combined.UserRoleMapping.RoleId,
                        r => r.RoleId.ToString(),
                        (combined, r) => new
                        {
                            combined.User.UserId,
                            combined.User.UserName,
                            combined.User.FirstName,
                            combined.User.LastName,
                            combined.User.IsActive,
                            combined.User.CreatedDate,
                            RoleName = r.RoleName,
                            r.Description
                        }
                    )
                    .FirstOrDefaultAsync();

                if (userDetails == null)
                {
                    // Handle case where user details with role information are not found
                    var response = new { success = false, message = "Failed to retrieve user details" };
                    return StatusCode(StatusCodes.Status500InternalServerError, response);
                }

                // Generate JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_config["JwtSettings:SecretKey"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, userDetails.FirstName),
                    new Claim(ClaimTypes.Name, userDetails.UserName),
                    new Claim(ClaimTypes.Role, userDetails.RoleName)
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var accessToken = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(accessToken);

                var responseSuccess = new
                {
                    success = true,
                    message = "Authentication successful",
                    accessToken = tokenString,
                    user = new
                    {
                        userDetails.UserId,
                        userDetails.UserName,
                        userDetails.FirstName,
                        userDetails.LastName,
                        userDetails.IsActive,
                        userDetails.RoleName,
                        userDetails.Description,
                        userDetails.CreatedDate
                        // Add other user details as needed
                    }
                };

                return Ok(responseSuccess);
            }
            catch (Exception ex)
            {
                var responseError = new { success = false, message = $"Server error: {ex.Message}" };
                return StatusCode(StatusCodes.Status500InternalServerError, responseError);
            }
        }

        // PUT: api/msg/updateuser
        [HttpPut]
        public async Task<IActionResult> UpdateUser(string userName, Users updateUserDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Find user by username
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

                if (user == null)
                {
                    return NotFound(new { success = false, message = "User does not exist" });
                }

                // Update user properties
                if (!string.IsNullOrEmpty(updateUserDto.FirstName))
                {
                    user.FirstName = updateUserDto.FirstName;
                }
                if (!string.IsNullOrEmpty(updateUserDto.LastName))
                {
                    user.LastName = updateUserDto.LastName;
                }
                if (updateUserDto.IsActive && User.IsInRole("superadmin"))
                {
                    user.IsActive = updateUserDto.IsActive;
                }
                if (!string.IsNullOrEmpty(updateUserDto.UserName) && User.IsInRole("superadmin"))
                {
                    user.UserName = updateUserDto.UserName;
                }

                // Update roleId in user role mapping if present
                var userRoleMapping = await _context.UserRoleMappings.FirstOrDefaultAsync(ur => ur.UserId == user.UserId);
                if (userRoleMapping != null)
                {
                    //userRoleMapping.RoleId = updateUserDto..RoleId;
                }
                else
                {
                    // Handle scenario where userRoleMapping does not exist (optional based on your application logic)
                    // This might occur if the user initially did not have a role mapping
                    // You may choose to create a new UserRoleMapping or handle this case as per your requirements
                    // Example:
                    // var newUserRoleMapping = new UserRoleMapping
                    // {
                    //     UserId = user.UserId,
                    //     RoleId = updateUserDto.UserRoleMapping.RoleId
                    // };
                    // _context.UserRoleMappings.Add(newUserRoleMapping);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { success = true, message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.Error.WriteLine($"Error updating user: {ex}");
                return StatusCode(500, new { success = false, message = "Internal Server Error" });
            }
        }


        [HttpDelete("deleteuser")]
        public async Task<IActionResult> DeleteUser(string userName)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Find user by userName
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

                    if (user == null)
                    {
                        return NotFound("User not found");
                    }

                    // Delete from UserRoleMapping table first
                    var userRoleMappings = _context.UserRoleMappings.Where(ur => ur.UserId == user.UserId);
                    _context.UserRoleMappings.RemoveRange(userRoleMappings);
                    await _context.SaveChangesAsync();

                    // Then delete from Users table
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();

                    // Commit transaction
                    transaction.Commit();

                    return Ok("User deleted successfully");
                }
                catch (Exception ex)
                {
                    // Rollback transaction if exception occurs
                    transaction.Rollback();
                    Console.Error.WriteLine($"Error deleting user: {ex}");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error");
                }
            }
        }


        private async Task<int?> GetRoleId(string roleName)
        {
            // Assuming _context is your DbContext instance

            // Query the Role table to find the RoleId corresponding to the RoleName
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);

            // Return the RoleId if found, otherwise return null
            return role?.RoleId;
        }


    }






    public class AuthRequest
     {
         public required string UserName { get; set; }
         public required string Password { get; set; }


     }
}