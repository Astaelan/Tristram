namespace Tristram.Shared.Network
{
    public enum EErrorCode : uint
    {
        Success = 0,
        ErrorDuringAccountLogin = 1,
        LoginInformationWasIncorrect = 3,
        RestrictedByParentalControls = 11,
        NoDiablo3GameAccount = 12,
        ProblemWithAnAuthenticationModule = 22,
        NewPatchAvailable = 28,
        LobbyDownForMaintenance = 33,
        TemporaryLobbyOutage = 35,
        UnableToDownloadAnAuthenticationModule = 36,
        ServersBusy = 37,
        BattleTagRequired = 38,
        InvalidServer = 42,
        MobilePhoneLocked = 43,
        AccountMuted = 44,
        GameTimeExpired = 50,
        SubscriptionExpired = 51,
        Banned = 52,
        Suspended = 53,
        DuplicateConnection = 60,
        DisconnectedFromService = 61,
        DisconnectedDueToDataChanges = 62,
        ThereWasAnError = 70,
        GoingDownForMaintenance = 71,
        GoingDownForPlannedMaintenance = 72,
        GameOutOfDate = 77,
    }
}
