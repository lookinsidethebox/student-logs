using Core.Enums;

namespace Core.Entities
{
	public class EducationMaterial : BaseObject
	{
		public string Title { get; set; } = null!;

		public string? Description { get; set; }

		public decimal Order { get; set; }

		public EducationMaterialType Type { get; set; }

		public int PageClickCount { get; set; }

		public int AnswersCount { get; set; }

		public int PlayStartCount { get; set; }

		public bool IsFirst { get; set; }

		public bool IsFinal { get; set; }

		public bool IsOneTime { get; set; }

		public bool IsRequireOtherMaterials { get; set; }

		public string? Text { get; set; }

		public string? FilePath { get; set; }

		public string? FileContentType { get; set; }

		public Survey? Survey { get; set; }
		public int? SurveyId { get; set; }
	}
}
