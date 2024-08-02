using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class smstemplateController : ControllerBase
    {
        private readonly MessagingDashboardContext _context;

        public smstemplateController(MessagingDashboardContext context)
        {
            _context = context;
        }
        [HttpPost("Createtemplate")]
        public async Task<IActionResult> CreateTemplate(string UserName, string RoleName, smsTemplate templateDto)
        {
            // Check if the user has the correct role
            if (RoleName != "superadmin")
            {
                return Unauthorized(new { message = "Only SuperAdmin can create templates." });
            }

            // Check if the template already exists
            var existingTemplate = await _context.SmsTemplate
                .FirstOrDefaultAsync(t => t.templateid == templateDto.templateid);

            if (existingTemplate != null)
            {
                return BadRequest(new { success = false, message = "TemplateId already exists" });
            }

            // Create the new template
            var template = new smsTemplate
            {
                templateid = templateDto.templateid,
                Content = templateDto.Content,
                CreatedBy = UserName,  // Use UserName from the method parameters
                CreatedDateTime = DateTime.UtcNow,
                Status = templateDto.Status
            };

            _context.SmsTemplate.Add(template);
            var result = await _context.SaveChangesAsync();

            // Return response based on the result
            if (result > 0)
            {
                return Ok(new { success = true, message = "Template created successfully" });
            }
            else
            {
                return StatusCode(500, new { success = false, message = "Failed to create template" });
            }
        }



        [HttpPut("UpdateTemplate/{templateId}")]
        public async Task<IActionResult> UpdateTemplate(String templateId, string UserName, string RoleName, smsTemplate updateDto)
        {
            try
            {
                if (RoleName != "superadmin" && RoleName != "admin")
                {
                    return Unauthorized(new { message = "Only SuperAdmin or Admin can update templates." });
                }


                // Check if templateId exists
                var existingTemplate = await _context.SmsTemplate
                    .FirstOrDefaultAsync(t => t.templateid == templateId);

                if (existingTemplate == null)
                {
                    return NotFound(new { success = false, message = "TemplateId not found" });
                }

                // Update template
                existingTemplate.Content = updateDto.Content;
                existingTemplate.UpdatedBy = UserName;
                existingTemplate.UpdatedDateTime = DateTime.UtcNow;
                existingTemplate.Status = updateDto.Status;

                _context.SmsTemplate.Update(existingTemplate);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { success = true, message = "Template updated successfully" });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = "Failed to update template" });
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error updating template: {ex}");
                return StatusCode(500, new { success = false, message = ex.Message ?? "Error updating template" });
            }
        }


        [HttpDelete("DeleteTemplate/{templateId}")]
        public async Task<IActionResult> DeleteTemplate(string templateId, [FromQuery] string UserName, [FromQuery] string RoleName)
        {
            try
            {
                // Check if user is authorized to delete templates
                if (RoleName != "superadmin")
                {
                    return NotFound(new { success = false, message = "rolename is not superadmin " });
                }

                // Check if templateId exists
                var existingTemplate = await _context.SmsTemplate
                    .FirstOrDefaultAsync(t => t.templateid == templateId);

                if (existingTemplate == null)
                {
                    return NotFound(new { success = false, message = "TemplateId not found" });
                }

                // Delete template
                _context.SmsTemplate.Remove(existingTemplate);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { success = true, message = "Template deleted successfully" });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = "Failed to delete template" });
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error deleting template: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Error deleting template" });
            }
        }


        [HttpGet("{templateId?}")]
        public async Task<IActionResult> GetTemplates(string? templateId)
        {
            try
            {
                if (!string.IsNullOrEmpty(templateId))
                {
                    // Fetch template by TemplateId if provided
                    var template = await _context.SmsTemplate
                        .Where(t => t.templateid == templateId)
                        .FirstOrDefaultAsync();

                    if (template == null)
                    {
                        var response1 = CreateResponseObject(true, "Template not found");
                        return NotFound(response1);
                    }

                    var response = CreateResponseObject(false, "Template retrieved", new { template });
                    return Ok(response);
                }
                else
                {
                    // Fetch all templates if TemplateId is not provided
                    var templates = await _context.SmsTemplate.ToListAsync();

                    var response = new
                    {
                        success = true,
                        templates,
                        message = "Fetched all templates successfully"
                    };

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                Console.Error.WriteLine($"Error fetching templates: {ex.Message}");

                var response = CreateResponseObject(true, "Internal Server Error", new { error = ex.Message });
                return StatusCode(500, response);
            }
        }

        private object CreateResponseObject(bool isError, string message, object? data = null)
        {
            return new
            {
                success = !isError,
                message,
                data
            };
        }
    


}
}

