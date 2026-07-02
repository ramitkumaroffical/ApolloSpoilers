using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApolloSpoilers.Application.DTOs
{
    public class ProductImageResultDto
    {
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}
