using HolidayApi.Domain;
using HolidayApi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;


namespace HolidayApi.Tests
{
    public class HolidayApiServiceTests
    {
        private HolidayContext dbContext;
        private HolidayService service;
        private int year;

        [SetUp]
        public void Setup()
        {
            year = Numerics.Year2025;
            // Create a new in-memory database for each test
            var options = new DbContextOptionsBuilder<HolidayContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            dbContext = new HolidayContext(options);
            var mockFactory = new Mock<IHttpClientFactory>();
            // _cache = new Mock<IMemoryCache>();
            // MockS

            service = new HolidayService(dbContext, mockFactory.Object);

        }

        [TearDown]
        public void TearDown()
        {
            dbContext.Dispose();
        }

        #region Positive Tests

        [Test]
        public async Task UpsertHolidaysFromApiAsync_SavesDataToDatabase()
        {
            string countryCode = CountryCodes.Netherlands;
            // Arrange: Add some holidays
            var holidays = new List<Holiday>
            {
                new () { Date = new DateTime(2025, 12, 25), Name = HolidayNames.Christmas, CountryCode = countryCode },
                new () { Date = new DateTime(2025, 4, 18), Name = HolidayNames.GoodFriday, CountryCode = countryCode }
            };
            dbContext.Holidays.AddRange(holidays);
            await dbContext.SaveChangesAsync();

            // Act
            var count = await dbContext.Holidays.CountAsync();

            // Assert
            Assert.That(count, Is.EqualTo(Numerics.Two));
        }

        [Test]
        public async Task GetPreviousThreeHolidays_ReturnsThreeMostRecent()
        {
            string countryCode = CountryCodes.UnitedStates;
            // Arrange: Add holidays for a country
            dbContext.Holidays.AddRange(
                new Holiday { Date = new DateTime(2025, 1, 1), Name = HolidayNames.NewYear, CountryCode = countryCode },
                new Holiday { Date = new DateTime(2025, 4, 18), Name = HolidayNames.GoodFriday, CountryCode = countryCode },
                new Holiday { Date = new DateTime(2025, 07, 04), Name = HolidayNames.IndependenceDay, CountryCode = countryCode },
                new Holiday { Date = new DateTime(2025, 9, 1), Name = HolidayNames.LaborDay, CountryCode = countryCode }
            );
            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.GetPreviousThreeHolidaysAsync(countryCode);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Count, Is.EqualTo(Numerics.Three));
                Assert.That(result.ToList()[Numerics.Two].Name, Is.EqualTo(HolidayNames.GoodFriday));
            });

        }

        [Test]
        public async Task GetNonWeekendHolidayCountAsync_ShouldReturnCorrectCounts()
        {
            // Arrange

            string firstCountryCode = CountryCodes.Austria;
            string secondCountryCode = CountryCodes.Canada;

            // Holidays (2 non-weekend, 1 weekend)
            dbContext.Holidays.AddRange(new List<Holiday>
            {
                new() { CountryCode = firstCountryCode, Date = new DateTime(2025, 1, 1) },  // Wednesday
                new() { CountryCode = firstCountryCode, Date = new DateTime(2025, 7, 4) },  // Friday
                new() { CountryCode = firstCountryCode, Date = new DateTime(2025, 7, 5) }   // Saturday
            });

            //  Holidays (1 non-weekend, 1 weekend)
            dbContext.Holidays.AddRange(new List<Holiday>
            {
                new() { CountryCode = secondCountryCode, Date = new DateTime(2025, 12, 25) }, // Thursday
                new() { CountryCode = secondCountryCode, Date = new DateTime(2025, 12, 28) }  // Sunday
            });

            await dbContext.SaveChangesAsync();

            var countryCodes = new[] { firstCountryCode, secondCountryCode };

            // Act
            var result = await service.GetNonWeekendHolidayCountAsync(year, countryCodes);

            // Assert

            var firstCountryResult = result.FirstOrDefault(r => r.CountryCode == firstCountryCode);
            var secondCountryResult = result.FirstOrDefault(r => r.CountryCode == secondCountryCode);

            Assert.Multiple(() =>
            {
                Assert.That(result.Count, Is.EqualTo(Numerics.Two));
                Assert.That(firstCountryResult, Is.Not.Null);
                Assert.That(secondCountryResult, Is.Not.Null);

                Assert.That(firstCountryResult!.Count, Is.EqualTo(Numerics.Two));
                Assert.That(secondCountryResult!.Count, Is.EqualTo(Numerics.One));
            });
        }

        [Test]
        public async Task GetSharedCelebrationDatesAsync_ShouldReturnCommonHolidaysBetweenTwoCountries()
        {
            // Arrange

            string countryA = CountryCodes.Germany;
            string countryB = CountryCodes.Belgium;


            dbContext.Holidays.AddRange(new List<Holiday>
            {
                // countryA Holidays
                new() { CountryCode = countryA, Date = new DateTime(year, 1, 1), Name = HolidayNames.NewYear, LocalName = HolidayNames.NewYearNameGermany },
                new() { CountryCode = countryA, Date = new DateTime(year, 3, 8), Name = HolidayNames.WomensDay, LocalName = HolidayNames.LocalWomensDayGermany },
                new() { CountryCode = countryA, Date = new DateTime(year, 12, 25), Name =HolidayNames.Christmas, LocalName = HolidayNames.LocalChristmasNameGermany },

                // countryB Holidays
                new() { CountryCode = countryB, Date = new DateTime(year, 1, 1),Name = HolidayNames.NewYear, LocalName = HolidayNames.NewYearNameBelgium },
                new() { CountryCode = countryB, Date = new DateTime(year, 11, 11), Name = HolidayNames.ArmisticeDay, LocalName = HolidayNames.LocalArmisticeDayBelgium },
                new() { CountryCode = countryB, Date = new DateTime(year, 12, 25),Name =HolidayNames.Christmas, LocalName = HolidayNames.LocalChristmasNameBelgium }
            });

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.GetSharedCelebrationDatesAsync(year, countryA, countryB);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Count, Is.EqualTo(Numerics.Two)); // Jan 1 & Dec 25 shared

            var newYear = result.FirstOrDefault(r => r.Date == new DateTime(year, 1, 1));
            var christmas = result.FirstOrDefault(r => r.Date == new DateTime(year, 12, 25));

            Assert.Multiple(() =>
            {
                Assert.That(newYear, Is.Not.Null);
                Assert.That(christmas, Is.Not.Null);

                Assert.That(newYear!.HolidayLocalNameFirstCountry, Is.EqualTo(HolidayNames.NewYearNameGermany));
                Assert.That(christmas!.HolidayLocalNameFirstCountry, Is.EqualTo(HolidayNames.LocalChristmasNameGermany));
                Assert.That(christmas!.HolidayLocalNameSecondCountry, Is.EqualTo(HolidayNames.LocalChristmasNameBelgium));
            });
        }

        #endregion

        #region Negative Tests

        [Test]
        public async Task UpsertHolidaysFromApiAsync_SavesDummyRecordToDatabase()
        {
            // Arrange
            // Act
            var count = await dbContext.Holidays.CountAsync();

            // Assert
            Assert.That(count, Is.EqualTo(Numerics.Zero));
        }

        [Test]
        public async Task GetPreviousThreeHolidays_ReturnsZeroMostRecent()
        {
            string countryCode = CountryCodes.UnitedStates;
            // Arrange: Add holidays for a country
            dbContext.Holidays.AddRange(
                new Holiday()
            );
            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.GetPreviousThreeHolidaysAsync(countryCode);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result?.Count, Is.EqualTo(Numerics.Zero));
            });
        }

        [Test]
        public async Task GetNonWeekendHolidayCountAsync_ShouldReturnZeroCount()
        {
            // Arrange

            string firstCountryCode = CountryCodes.Austria;
            string secondCountryCode = CountryCodes.Canada;

            dbContext.Holidays.AddRange(new List<Holiday>
            {
                new() { CountryCode = firstCountryCode, Date = new DateTime(2025, 7, 5) }   // Saturday
            });


            dbContext.Holidays.AddRange(new List<Holiday>
            {
                new() { CountryCode = secondCountryCode, Date = new DateTime(2025, 12, 28) }  // Sunday
            });

            await dbContext.SaveChangesAsync();

            var countryCodes = new[] { firstCountryCode, secondCountryCode };

            // Act
            var result = await service.GetNonWeekendHolidayCountAsync(year, countryCodes);

            // Assert
            Assert.That(result?.Count, Is.EqualTo(Numerics.Two));

            var firstCountryResult = result.FirstOrDefault(r => r.CountryCode == firstCountryCode);
            var secondCountryResult = result.FirstOrDefault(r => r.CountryCode == secondCountryCode);

            Assert.Multiple(() =>
            {
                Assert.That(firstCountryResult, Is.Not.Null);
                Assert.That(secondCountryResult, Is.Not.Null);

                Assert.That(firstCountryResult!.Count, Is.EqualTo(Numerics.Zero));
                Assert.That(secondCountryResult!.Count, Is.EqualTo(Numerics.Zero));
            });
        }

        [Test]
        public async Task GetSharedCelebrationDatesAsync_ShouldReturnEmpty_WhenNoSharedDates()
        {
            // Arrange
            string countryA = CountryCodes.Germany;
            string countryB = CountryCodes.Belgium;

            dbContext.Holidays.AddRange(new List<Holiday>
            {
               new() { CountryCode = countryA, Date = new DateTime(year, 3, 8), Name = HolidayNames.WomensDay, LocalName = HolidayNames.LocalWomensDayGermany }
            });

            await dbContext.SaveChangesAsync();

            // Act
            var result = await service.GetSharedCelebrationDatesAsync(year, countryA, countryB);

            // Assert
            Assert.That(result, Is.Empty);
        }
        #endregion
    }
}
