namespace ApolloSpoilers.Domain.Enums;

/// <summary>Origin of a vectorized knowledge chunk used by Aasra's RAG pipeline.</summary>
public enum KnowledgeSourceType
{
    Product = 0,
    Category = 1,
    Faq = 2,
    Policy = 3
}
