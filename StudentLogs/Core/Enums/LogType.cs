using Core.Attributes;

namespace Core.Enums
{
	public enum LogType
	{
		[StringValue("Не задано")]
		NotSet = 0,

		[StringValue("Переход на страницу")]
		Clicked = 10,

		[StringValue("Скачивание документа")]
		Downloaded = 20,

		[StringValue("Воспроизведение видео")]
		VideoStarted = 30,

		[StringValue("Остановка воспроизведения видео")]
		VideoStopped = 40,

		[StringValue("Изменение скорости видео")]
		VideoSpeedChanged = 50,

		[StringValue("Заполнение опроса")]
		SurveyCompleted = 60
	}
}
