using BookHub.Application.Common;

namespace BookHub.Tests.Application
{
    public class ListQueryTests
    {
        [Fact]
        public void Normalize_ClampsPageAndPageSizeToValidRanges()
        {
            var query = new ListQuery("  duna ", Page: -3, PageSize: 5000).Normalize();

            Assert.Equal("duna", query.Search);
            Assert.Equal(1, query.Page);
            Assert.Equal(Paging.MaxPageSize, query.PageSize);
        }

        [Fact]
        public void Normalize_WithHugePage_KeepsTheOffsetWithinIntRange()
        {
            var query = new ListQuery(Page: int.MaxValue, PageSize: Paging.MaxPageSize).Normalize();

            Assert.Equal(Paging.MaxPage, query.Page);
            Assert.True(query.Skip >= 0);
        }

        [Fact]
        public void BookListQuery_Normalize_WithHugePage_KeepsTheOffsetWithinIntRange()
        {
            var query = new BookListQuery(Page: int.MaxValue, PageSize: Paging.MaxPageSize, SortBy: " YEAR ").Normalize();

            Assert.Equal(Paging.MaxPage, query.Page);
            Assert.True(query.Skip >= 0);
            Assert.Equal("year", query.SortBy);
        }
    }
}
