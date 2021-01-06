namespace DeltaX.LinSql.Table
{
    class TableConfigurationIdentifierCreator
    {
        private static int identifierCount = 1;

        public static string GetIdentifier()
        {
            return $"t_{identifierCount++}";
        }
    }
}