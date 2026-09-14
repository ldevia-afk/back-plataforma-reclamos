namespace GestionCasos.Web.Models;

public enum UserProfileType
{
    Cliente = 0,
    Interno = 1,
    /// <summary>Ve y administra todo, de todos los países.</summary>
    Administrador = 2,
    /// <summary>Ve y administra todo, pero sólo dentro de su propio país.</summary>
    AdministradorPais = 3
}

public static class UserProfileTypeExtensions
{
    public static string ToDisplayName(this UserProfileType profileType) => profileType switch
    {
        UserProfileType.Cliente => "Cliente",
        UserProfileType.Interno => "Interno",
        UserProfileType.Administrador => "Administrador",
        UserProfileType.AdministradorPais => "Administrador de país",
        _ => profileType.ToString()
    };

    /// <summary>True para los perfiles que gestionan casos (Interno y los dos administradores).</summary>
    public static bool IsInternalStaff(this UserProfileType profileType) => profileType != UserProfileType.Cliente;

    /// <summary>True para los perfiles con permisos de administración (usuarios, clientes, sucursales, categorías).</summary>
    public static bool IsAdmin(this UserProfileType profileType) =>
        profileType is UserProfileType.Administrador or UserProfileType.AdministradorPais;
}
