using Hongdal.Domain.Education;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hongdal.Infrastructure.Persistence.Configurations.Education;

public sealed class 교육과정Configuration : IEntityTypeConfiguration<교육과정>
{
    public void Configure(EntityTypeBuilder<교육과정> builder)
    {
        builder.ToTable("education_courses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.과정코드).HasColumnName("course_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.과정명).HasColumnName("course_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.설명).HasColumnName("description").HasColumnType("text").IsRequired();
        builder.Property(x => x.운영방식).HasColumnName("delivery_mode").HasMaxLength(100).IsRequired();
        builder.Property(x => x.최소이수개월).HasColumnName("minimum_months");
        builder.Property(x => x.활성화여부).HasColumnName("is_active");
        builder.Property(x => x.출처Url).HasColumnName("source_url").HasMaxLength(1000);
        builder.Property(x => x.생성일시Utc).HasColumnName("created_at_utc");
        builder.Property(x => x.수정일시Utc).HasColumnName("updated_at_utc");
        builder.HasIndex(x => x.과정코드).IsUnique();
        builder.HasIndex(x => new { x.활성화여부, x.과정명 });
    }
}

public sealed class 교육과정과목Configuration : IEntityTypeConfiguration<교육과정과목>
{
    public void Configure(EntityTypeBuilder<교육과정과목> builder)
    {
        builder.ToTable("education_course_subjects");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.교육과정Id).HasColumnName("course_id");
        builder.Property(x => x.과목코드).HasColumnName("subject_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.과목명).HasColumnName("subject_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.표시순서).HasColumnName("display_order");
        builder.Property(x => x.최소참석횟수).HasColumnName("minimum_attendance_count");
        builder.HasOne(x => x.교육과정)
            .WithMany(x => x.과목목록)
            .HasForeignKey(x => x.교육과정Id)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.교육과정Id, x.과목코드 }).IsUnique();
    }
}

public sealed class 교육과정양식Configuration : IEntityTypeConfiguration<교육과정양식>
{
    public void Configure(EntityTypeBuilder<교육과정양식> builder)
    {
        builder.ToTable("education_course_forms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.교육과정Id).HasColumnName("course_id");
        builder.Property(x => x.양식코드).HasColumnName("form_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.양식명).HasColumnName("form_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.목적).HasColumnName("purpose").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.버전).HasColumnName("version").HasMaxLength(50).IsRequired();
        builder.Property(x => x.제출주기).HasColumnName("submission_cycle").HasMaxLength(100).IsRequired();
        builder.Property(x => x.최소제출횟수).HasColumnName("minimum_submission_count");
        builder.Property(x => x.필수여부).HasColumnName("is_required");
        builder.Property(x => x.활성화여부).HasColumnName("is_active");
        builder.Property(x => x.필드정의Json).HasColumnName("field_definition_json").HasColumnType("longtext").IsRequired();
        builder.Property(x => x.출처Url).HasColumnName("source_url").HasMaxLength(1000);
        builder.Property(x => x.생성일시Utc).HasColumnName("created_at_utc");
        builder.Property(x => x.수정일시Utc).HasColumnName("updated_at_utc");
        builder.HasOne(x => x.교육과정)
            .WithMany(x => x.양식목록)
            .HasForeignKey(x => x.교육과정Id)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.교육과정Id, x.양식코드 }).IsUnique();
    }
}

public sealed class 교육과정신청Configuration : IEntityTypeConfiguration<교육과정신청>
{
    public void Configure(EntityTypeBuilder<교육과정신청> builder)
    {
        builder.ToTable("education_course_applications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.교육과정Id).HasColumnName("course_id");
        builder.Property(x => x.신청자UserId).HasColumnName("applicant_user_id").HasMaxLength(450).IsRequired();
        ConfigureEncrypted(builder.Property(x => x.이름암호문), "name_ciphertext");
        ConfigureEncrypted(builder.Property(x => x.별명암호문), "nickname_ciphertext");
        ConfigureEncrypted(builder.Property(x => x.이메일암호문), "email_ciphertext");
        ConfigureEncrypted(builder.Property(x => x.전화번호암호문), "phone_ciphertext");
        ConfigureEncrypted(builder.Property(x => x.성별암호문), "gender_ciphertext");
        ConfigureEncrypted(builder.Property(x => x.출생연도암호문), "birth_year_ciphertext");
        ConfigureEncrypted(builder.Property(x => x.거주국가암호문), "country_ciphertext");
        builder.Property(x => x.회원가입확인).HasColumnName("membership_confirmed");
        builder.Property(x => x.입교서약동의).HasColumnName("entry_pledge_agreed");
        builder.Property(x => x.개인정보수집이용동의).HasColumnName("personal_data_agreed");
        builder.Property(x => x.개인정보제3자제공동의).HasColumnName("third_party_data_agreed");
        builder.Property(x => x.개인정보동의버전).HasColumnName("personal_data_consent_version").HasMaxLength(50).IsRequired();
        builder.Property(x => x.제3자제공동의버전).HasColumnName("third_party_consent_version").HasMaxLength(50).IsRequired();
        builder.Property(x => x.동의일시Utc).HasColumnName("consented_at_utc");
        builder.Property(x => x.상태).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.심사자UserId).HasColumnName("reviewer_user_id").HasMaxLength(450);
        ConfigureEncrypted(builder.Property(x => x.심사메모암호문), "review_note_ciphertext");
        builder.Property(x => x.신청일시Utc).HasColumnName("applied_at_utc");
        builder.Property(x => x.심사일시Utc).HasColumnName("reviewed_at_utc");
        builder.Property(x => x.개인정보삭제일시Utc).HasColumnName("personal_data_deleted_at_utc");
        builder.HasOne(x => x.교육과정)
            .WithMany()
            .HasForeignKey(x => x.교육과정Id)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.교육과정Id, x.신청자UserId, x.상태 });
        builder.HasIndex(x => new { x.상태, x.신청일시Utc });
    }

    private static void ConfigureEncrypted(PropertyBuilder<string> property, string columnName)
        => property.HasColumnName(columnName).HasColumnType("longtext").IsRequired();
}

public sealed class 교육과정등록Configuration : IEntityTypeConfiguration<교육과정등록>
{
    public void Configure(EntityTypeBuilder<교육과정등록> builder)
    {
        builder.ToTable("education_course_enrollments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.교육과정Id).HasColumnName("course_id");
        builder.Property(x => x.교육과정신청Id).HasColumnName("application_id");
        builder.Property(x => x.참여자UserId).HasColumnName("participant_user_id").HasMaxLength(450).IsRequired();
        builder.Property(x => x.담당멘토UserId).HasColumnName("mentor_user_id").HasMaxLength(450);
        builder.Property(x => x.상태).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.시작일시Utc).HasColumnName("started_at_utc");
        builder.Property(x => x.종료일시Utc).HasColumnName("ended_at_utc");
        builder.Property(x => x.생성일시Utc).HasColumnName("created_at_utc");
        builder.HasOne(x => x.교육과정)
            .WithMany()
            .HasForeignKey(x => x.교육과정Id)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.교육과정신청)
            .WithOne(x => x.등록)
            .HasForeignKey<교육과정등록>(x => x.교육과정신청Id)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.교육과정신청Id).IsUnique();
        builder.HasIndex(x => new { x.참여자UserId, x.상태 });
    }
}

public sealed class 교육과정참석기록Configuration : IEntityTypeConfiguration<교육과정참석기록>
{
    public void Configure(EntityTypeBuilder<교육과정참석기록> builder)
    {
        builder.ToTable("education_course_attendances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.교육과정등록Id).HasColumnName("enrollment_id");
        builder.Property(x => x.교육과정과목Id).HasColumnName("subject_id");
        builder.Property(x => x.회차Key).HasColumnName("session_key").HasMaxLength(100).IsRequired();
        builder.Property(x => x.회차명).HasColumnName("session_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.수업일시Utc).HasColumnName("session_at_utc");
        builder.Property(x => x.참석여부).HasColumnName("attended");
        builder.Property(x => x.기록자UserId).HasColumnName("recorded_by_user_id").HasMaxLength(450).IsRequired();
        builder.Property(x => x.기록일시Utc).HasColumnName("recorded_at_utc");
        builder.HasOne(x => x.교육과정등록)
            .WithMany(x => x.참석목록)
            .HasForeignKey(x => x.교육과정등록Id)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.교육과정과목)
            .WithMany()
            .HasForeignKey(x => x.교육과정과목Id)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.교육과정등록Id, x.교육과정과목Id, x.회차Key }).IsUnique();
    }
}

public sealed class 교육과정과제제출Configuration : IEntityTypeConfiguration<교육과정과제제출>
{
    public void Configure(EntityTypeBuilder<교육과정과제제출> builder)
    {
        builder.ToTable("education_course_submissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.교육과정등록Id).HasColumnName("enrollment_id");
        builder.Property(x => x.교육과정양식Id).HasColumnName("form_id");
        builder.Property(x => x.제출기간Key).HasColumnName("period_key").HasMaxLength(100).IsRequired();
        builder.Property(x => x.답변암호문).HasColumnName("answers_ciphertext").HasColumnType("longtext").IsRequired();
        builder.Property(x => x.상태).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.확인자UserId).HasColumnName("reviewer_user_id").HasMaxLength(450);
        builder.Property(x => x.확인메모암호문).HasColumnName("review_note_ciphertext").HasColumnType("longtext").IsRequired();
        builder.Property(x => x.제출일시Utc).HasColumnName("submitted_at_utc");
        builder.Property(x => x.확인일시Utc).HasColumnName("reviewed_at_utc");
        builder.HasOne(x => x.교육과정등록)
            .WithMany(x => x.제출목록)
            .HasForeignKey(x => x.교육과정등록Id)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.교육과정양식)
            .WithMany(x => x.제출목록)
            .HasForeignKey(x => x.교육과정양식Id)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.교육과정등록Id, x.교육과정양식Id, x.제출기간Key }).IsUnique();
        builder.HasIndex(x => new { x.상태, x.제출일시Utc });
    }
}
