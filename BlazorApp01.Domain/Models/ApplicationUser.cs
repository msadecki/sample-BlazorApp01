using BlazorApp01.Domain.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace BlazorApp01.Domain.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public sealed class ApplicationUser : IdentityUser, IEntity
{ }