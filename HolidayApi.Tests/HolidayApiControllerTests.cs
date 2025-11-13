using Moq;
using Microsoft.AspNetCore.Mvc;
using HolidayApi.Domain;
using HolidayApi.Controllers;
using HolidayApi.Application;


namespace HolidayApi.Tests
{
    [TestFixture]
    public class HolidayApiControllerTests
    {
        private Mock<IHolidayService> mockService;
        private HolidaysController controller;

        private int year;

        [SetUp]
        public void SetUp()
        {
            year = Numerics.Year2025;
            mockService = new Mock<IHolidayService>();
            controller = new HolidaysController(mockService.Object);
        }

        #region Positive Tests
        [Test]
        public async Task UpsertHolidays_ShouldReturnOk_WithExpectedMessage()
        {
            // Arrange
            string country = CountryCodes.Netherlands;

            // Mock service to return some inserted holidays
            var insertedHolidays = new List<UpsertHolidayDto>
            {
                new() { Date = new DateTime(2025, 12, 25), Name = HolidayNames.Christmas, CountryCode = country },
                new() { Date = new DateTime(2025, 4, 18), Name = HolidayNames.GoodFriday, CountryCode = country }
            };

            mockService
                .Setup(s => s.InsertOrUpdateHolidaysFromApiAsync(year, country))
                .ReturnsAsync(insertedHolidays);

            // Act
            var result = await controller.InsertOrUpdateHolidays(year, country);

            // Assert
            // Ensure it returns OkObjectResult
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            // Extract the strongly typed response DTO
            var response = okResult!.Value as UpsertHolidayResponseDto;
            Assert.That(response, Is.Not.Null);

            Assert.Multiple(() =>
            {
                // Check the message
                Assert.That(response!.Message, Is.EqualTo(InformationMessages.HolidaysUpdated));

                // Check the inserted records
                Assert.That(response.Result.Count, Is.EqualTo(insertedHolidays.Count));
            });
            Assert.That(response.Result[Numerics.Zero].Name, Is.EqualTo(HolidayNames.Christmas));

            mockService.Verify(s => s.InsertOrUpdateHolidaysFromApiAsync(year, country), Times.Once);
        }

        [Test]
        public async Task GetPreviousThreeHolidays_ShouldReturnOk_WithExpectedData()
        {
            // Arrange
            string countryCode = CountryCodes.UnitedStates;
            var expected = new List<PreviousHolidayDto>
            {
                new () { Date = new DateTime(2025, 1, 1), Name = HolidayNames.NewYear },
                new () { Date = new DateTime(2025, 4, 18), Name = HolidayNames.GoodFriday},
                new () { Date = new DateTime(2025, 07, 04), Name = HolidayNames.IndependenceDay },
                new () { Date = new DateTime(2025, 9, 1), Name = HolidayNames.LaborDay }
            };


            mockService.Setup(s => s.GetPreviousThreeHolidaysAsync(countryCode)).ReturnsAsync(expected);

            // Act
            var result = await controller.GetPreviousThreeHolidays(countryCode);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var response = okResult!.Value as PreviousHolidayResponseDto;
            Assert.That(response, Is.Not.Null);

            var topThreeResults = response.Result.Take(Numerics.Three);
            Assert.Multiple(() =>
            {
                Assert.That(response.Message, Is.EqualTo(InformationMessages.RecordsFound));
                Assert.That(topThreeResults.Count(), Is.EqualTo(Numerics.Three));
                Assert.That(topThreeResults.First().Name, Is.EqualTo(HolidayNames.NewYear));
            });


            mockService.Verify(s => s.GetPreviousThreeHolidaysAsync(countryCode), Times.Once);
        }

        [Test]
        public async Task GetNonWeekendHolidayCount_ShouldReturnOk_WithExpectedCounts()
        {
            // Arrange

            var countries = new[] { CountryCodes.Austria, CountryCodes.Canada };
            var expected = new List<NonWeekendHolidayDto>
                            {
                                new() { CountryCode = CountryCodes.Austria, Count = Numerics.Two },
                                new() { CountryCode = CountryCodes.Canada, Count = Numerics.Four }
                            };

            mockService
                .Setup(s => s.GetNonWeekendHolidayCountAsync(year, countries))
                .ReturnsAsync(expected);

            // Act
            var result = await controller.GetNonWeekendHolidayCount(year, countries);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as NonWeekendHolidayResponseDto;

            Assert.That(response, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(response!.Result.Count, Is.EqualTo(Numerics.Two));
                Assert.That(response.Result[0].CountryCode, Is.EqualTo(CountryCodes.Austria));
                Assert.That(response.Result[0].Count, Is.EqualTo(Numerics.Two));
                Assert.That(response.Message, Is.EqualTo(InformationMessages.RecordsFound));
            });

            mockService.Verify(s => s.GetNonWeekendHolidayCountAsync(year, countries), Times.Once);
        }

        [Test]
        public async Task GetSharedHolidays_ShouldReturnOk_WithExpectedSharedDates()
        {
            // Arrange

            string firstCountryCode = CountryCodes.Germany;
            string secondCountryCode = CountryCodes.Belgium;
            var expected = new List<SharedHolidayDto>
            {
                new SharedHolidayDto {Date = new DateTime(2025, 1, 1), HolidayLocalNameFirstCountry = firstCountryCode, HolidayLocalNameSecondCountry = secondCountryCode }
            };

            // _mockService.Setup(s => s.GetSharedCelebrationDatesAsync(year, countryA, countryB)).ReturnsAsync(expected);
            mockService.Setup(s => s.GetSharedCelebrationDatesAsync(year, firstCountryCode, secondCountryCode)).ReturnsAsync(expected);
            // Act
            var result = await controller.GetSharedHolidays(year, firstCountryCode, secondCountryCode);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as SharedHolidayResponseDto;

            Assert.That(response, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(response.Result.Count, Is.EqualTo(Numerics.One));
                Assert.That(response.Message, Is.EqualTo(InformationMessages.RecordsFound));
            });

            mockService.Verify(s => s.GetSharedCelebrationDatesAsync(year, firstCountryCode, secondCountryCode), Times.Once);
        }

        #endregion

        #region Negative Tests

        [Test]
        public async Task UpsertHolidays_ShouldReturnOk_WithZeroRecordsUpsert()
        {
            // Arrange
            string country = CountryCodes.Netherlands;

            // Mock service to return some inserted holidays
            var insertedHolidays = new List<UpsertHolidayDto>();

            mockService
                .Setup(s => s.InsertOrUpdateHolidaysFromApiAsync(year, country))
                .ReturnsAsync(insertedHolidays);

            // Act
            var result = await controller.InsertOrUpdateHolidays(year, country);

            // Assert

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var response = okResult!.Value as UpsertHolidayResponseDto;
            Assert.That(response, Is.Not.Null);

            Assert.Multiple(() =>
            {
                // Check the message and records
                Assert.That(response.Result.Count, Is.EqualTo(insertedHolidays.Count));
                Assert.That(response.Message, Is.EqualTo(InformationMessages.NoHolidaysUpdated));
            });

            mockService.Verify(s => s.InsertOrUpdateHolidaysFromApiAsync(year, country), Times.Once);
        }


        [Test]
        public async Task GetPreviousThreeHolidays_ShouldReturnOk_WithNoData()
        {
            // Arrange
            string countryCode = CountryCodes.UnitedStates;
            var expected = new List<PreviousHolidayDto>();

            mockService.Setup(s => s.GetPreviousThreeHolidaysAsync(countryCode)).ReturnsAsync(expected);

            // Act
            var result = await controller.GetPreviousThreeHolidays(countryCode);


            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var response = okResult!.Value as PreviousHolidayResponseDto;
            Assert.That(response, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(response.Result.Count, Is.EqualTo(Numerics.Zero));
                Assert.That(response.Message, Is.EqualTo(InformationMessages.NoRecordsFound));
            });

            mockService.Verify(s => s.GetPreviousThreeHolidaysAsync(countryCode), Times.Once);
        }

        [Test]
        public async Task GetNonWeekendHolidayCount_ShouldReturnOk_WithZeroRecords()
        {
            // Arrange

            var countries = new[] { CountryCodes.Austria, CountryCodes.Canada };
            var expected = new List<NonWeekendHolidayDto>();

            mockService
                .Setup(s => s.GetNonWeekendHolidayCountAsync(year, countries))
                .ReturnsAsync(expected);

            // Act
            var result = await controller.GetNonWeekendHolidayCount(year, countries);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as NonWeekendHolidayResponseDto;

            Assert.That(response, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(response!.Result.Count, Is.EqualTo(Numerics.Zero));
                Assert.That(response.Message, Is.EqualTo(InformationMessages.NoRecordsFound));
            });

            mockService.Verify(s => s.GetNonWeekendHolidayCountAsync(year, countries), Times.Once);
        }

        [Test]
        public async Task GetSharedHolidays_ShouldReturnOk_WithZeroSharedDates()
        {
            // Arrange

            string firstCountryCode = CountryCodes.Germany;
            string secondCountryCode = CountryCodes.Belgium;
            var expected = new List<SharedHolidayDto>();

            mockService.Setup(s => s.GetSharedCelebrationDatesAsync(year, firstCountryCode, secondCountryCode)).ReturnsAsync(expected);
            // Act
            var result = await controller.GetSharedHolidays(year, firstCountryCode, secondCountryCode);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as SharedHolidayResponseDto;

            Assert.That(response, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(response.Result.Count, Is.EqualTo(Numerics.Zero));
                Assert.That(response.Message, Is.EqualTo(InformationMessages.NoRecordsFound));
            });

            mockService.Verify(s => s.GetSharedCelebrationDatesAsync(year, firstCountryCode, secondCountryCode), Times.Once);
        }
        #endregion
    }
}
