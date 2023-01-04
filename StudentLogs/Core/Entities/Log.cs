using Core.Enums;

namespace Core.Entities
{
	public class Log : BaseObject
	{
		public User User { get; set; } = null!;
		public int UserId { get; set; }

		public EducationMaterial EducationMaterial { get; set; } = null!;
		public int EducationMaterialId { get; set; }

		public DateTime CreateDate { get; set; }

		public LogType Type { get; set; }

		public string? Info { get; set; }
	}
}
