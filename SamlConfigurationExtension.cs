using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;

namespace POC_Saml;

public static class SamlConfigurationExtension
{
    public static void ConfigureSaml2(this IServiceCollection services, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        services.Configure<Saml2Configuration>(configuration.GetSection("Saml2"));

        services.Configure<Saml2Configuration>(saml2Configuration =>
        {
            saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);
            
            var uri = configuration["Saml2:IdPMetadata"];

            var entityDescriptor = new EntityDescriptor();
            entityDescriptor.ReadIdPSsoDescriptorFromUrlAsync(httpClientFactory, new Uri(uri)).GetAwaiter().GetResult();
            if (entityDescriptor.IdPSsoDescriptor != null)
            {
                saml2Configuration.SingleSignOnDestination =
                    entityDescriptor.IdPSsoDescriptor.SingleSignOnServices.First().Location;
                saml2Configuration.SingleLogoutDestination =
                    entityDescriptor.IdPSsoDescriptor.SingleLogoutServices.First().Location;
                saml2Configuration.SignatureValidationCertificates.AddRange(entityDescriptor.IdPSsoDescriptor
                    .SigningCertificates);
            }
            else
            {
                throw new Exception("IdPSsoDescriptor not loaded from metadata.");
            }
        });

        services.AddSaml2();
    }
}