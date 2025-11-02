using EasyReasy.EnvironmentVariables;

namespace RoomReservationApi.Tests
{
    [EnvironmentVariableNameContainer]
    public static class EnvironmentVariables
    {
        [EnvironmentVariableName(minLength: 6)]
        public static readonly VariableName KthApiKey = new VariableName("KTH_API_KEY");
    }
}
