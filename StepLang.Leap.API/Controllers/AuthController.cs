using System.Security.Claims;
using Leap.API.DB;
using Leap.API.DB.Entities;
using Leap.API.Interfaces;
using Leap.API.Services;
using Leap.Common.DTO.API;
using Leap.Common.Validators.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Leap.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController
(
	LeapApiDbContext context,
	ITokenGenerator generator,
	IPasswordHasher<Author> hasher,
	ILogger<AuthController> logger,
	IValidator<RegisterRequest> registerRequestValidator
) : ControllerBase
{
	[HttpPost]
	[ProducesResponseType(typeof(RegisterResult), StatusCodes.Status409Conflict)]
	[ProducesResponseType(typeof(RegisterResult), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(typeof(RegisterResult), StatusCodes.Status200OK)]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request)
	{
		logger.LogTrace("Incoming register request");

		var existingAuthor = await context.Authors.FirstOrDefaultAsync(u => u.Username == request.Username);
		if (existingAuthor is not null)
		{
			logger.LogInformation(
				"Rejected register request because of a duplicate username ({Username})",
				request.Username
			);

			return Conflict(RegisterResult.UsernameExists());
		}

		logger.LogTrace("Validating register request");

		if (!registerRequestValidator.IsValid(request))
		{
			var errors = registerRequestValidator.GetErrors(request).ToList();

			logger.LogInformation("Rejected register request because of invalid data ({Errors})", errors);

			return UnprocessableEntity(RegisterResult.InvalidRequest(errors.Select(e => e.ToString())));
		}

		var author = new Author
		{
			Username = request.Username,
		};

		var passwordHash = hasher.HashPassword(author, request.Password);
		author.PasswordHash = passwordHash;

		logger.LogTrace("Adding new author to DB (Username: {Username})", author.Username);

		var entity = await context.Authors.AddAsync(author);
		await context.SaveChangesAsync();

		logger.LogInformation("New author registered: {Author}", entity.Entity);

		var token = generator.Create(author);

		logger.LogTrace("Returning token after successful registration");

		return Ok(RegisterResult.Success(token));
	}

	[HttpPost]
	[ProducesResponseType(typeof(CreateTokenResult), StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(typeof(CreateTokenResult), StatusCodes.Status200OK)]
	public async Task<IActionResult> Token([FromBody] CreateTokenRequest request)
	{
		logger.LogTrace("Incoming create token request");

		var author = await context.Authors.FirstOrDefaultAsync(u => u.Username == request.Username);
		if (author is null)
		{
			logger.LogTrace(
				"Rejected create token request because no author with the username '{Username}' could be found",
				request.Username
			);

			return Unauthorized(CreateTokenResult.Unauthorized());
		}

		var result = hasher.VerifyHashedPassword(author, author.PasswordHash, request.Password);
		if (result == PasswordVerificationResult.Failed)
		{
			logger.LogTrace(
				"Rejected create token request because password validation failed for username {Username}",
				request.Username
			);

			return Unauthorized(CreateTokenResult.Unauthorized());
		}

		logger.LogTrace("Password validation succeeded for author {Author}", author);

		if (result == PasswordVerificationResult.SuccessRehashNeeded)
		{
			logger.LogTrace("Password hash needs to be updated. Rehashing password for {Author}", author);

			author.PasswordHash = hasher.HashPassword(author, request.Password);
			await context.SaveChangesAsync();
		}

		var token = generator.Create(author);

		logger.LogTrace("Returning token after successful create token request");

		return Ok(CreateTokenResult.Success(token));
	}

	[HttpGet]
	[ProducesResponseType(typeof(AuthCheckResult), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(AuthCheckResult), StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(typeof(AuthCheckResult), StatusCodes.Status403Forbidden)]
	[ProducesResponseType(typeof(AuthCheckResult), StatusCodes.Status200OK)]
	public IActionResult Check()
	{
		logger.LogTrace("Incoming check authentication request");

		var idStr = User.FindFirstValue(TokenGenerator.IdClaim);
		if (idStr is null)
		{
			logger.LogTrace("Check authentication request failed because of a missing ID claim (probably no JWT)");

			return Unauthorized(AuthCheckResult.NoIdClaim());
		}

		if (!Guid.TryParse(idStr, out var id))
		{
			logger.LogTrace(
				"Check authentication request failed because of an invalid id in claim ('{Id}' could not be parsed as a GUID)",
				idStr
			);

			return BadRequest(AuthCheckResult.InvalidIdClaim());
		}

		var author = context.Authors.Find(id);
		if (author is null)
		{
			logger.LogTrace(
				"Check authentication request failed because no user with the given ID could be found (even though the claim was valid, ID: {Id})",
				id
			);

			return StatusCode(StatusCodes.Status403Forbidden, AuthCheckResult.NoAuthor());
		}

		logger.LogTrace("Check authentication request succeeded for author {Author}", author);

		return Ok(AuthCheckResult.Success(author.Username));
	}
}
