using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.업체
{
    [Table("업체")]
    public class 업체
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("notion_page_id")]
        public string NotionPageId { get; set; } = string.Empty;

        [Column("업체명")]
        public string 업체명 { get; set; } = string.Empty;

        [Column("상태")]
        public string 상태 { get; set; } = "거래중";

        [Column("대표_연락처")]
        public string 대표_연락처 { get; set; } = string.Empty;

        [Column("담당자")]
        public string 담당자 { get; set; } = string.Empty;

        [Column("이메일")]
        public string 이메일 { get; set; } = string.Empty;

        [Column("주소")]
        public string 주소 { get; set; } = string.Empty;

        [Column("정산_결제_조건")]
        public string 정산_결제_조건 { get; set; } = string.Empty;

        [Column("첨부_json")]
        public string 첨부_json { get; set; } = "[]";

        [Column("메모")]
        public string 메모 { get; set; } = string.Empty;

        [Column("등록일")]
        public DateTime? 등록일 { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
