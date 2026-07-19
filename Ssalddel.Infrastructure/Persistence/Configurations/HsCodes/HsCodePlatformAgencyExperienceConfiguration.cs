using Ssalddel.Domain.HsCodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssalddel.Infrastructure.Persistence.Configurations.HsCodes;

public sealed class HsCodePlatformAgencyExperienceConfiguration : IEntityTypeConfiguration<HsCodePlatformAgencyExperience>
{
    public void Configure(EntityTypeBuilder<HsCodePlatformAgencyExperience> builder)
    {
        builder.ToTable("hs_code_platform_agency_experiences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.HsCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.AgencyType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CountryRoute).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CaseStatus).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RiskLevel).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.RequiredDocumentsJson).HasColumnType("json").IsRequired();
        builder.Property(x => x.ContributorUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.PaidAccessPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ContributorRewardRate).HasColumnType("decimal(9,6)");
        builder.Property(x => x.DisclosurePolicy).HasMaxLength(2000).IsRequired();

        builder.HasIndex(x => new { x.HsCode, x.AgencyType, x.CountryRoute });
        builder.HasIndex(x => new { x.HsCode, x.ContributorConsented, x.IsPaidDetail });
        builder.HasIndex(x => new { x.ContributorUserId, x.ContributorConsented });
    }
}
