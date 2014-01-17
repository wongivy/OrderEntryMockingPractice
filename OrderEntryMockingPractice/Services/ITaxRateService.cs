using System.Collections.Generic;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public interface ITaxRateService
    {
        IEnumerable<TaxEntry> GetTaxEntries(string postalCode, string country);
    }
}