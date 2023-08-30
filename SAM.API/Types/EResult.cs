using System.ComponentModel;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace SAM.API.Types;

/// <summary>
/// Steam error result codes.
/// </summary>
/// <see href="https://partner.steamgames.com/doc/api/steam_api#EResult" />
public enum EResult
{
    [Description("Success.")]
    k_EResultOK = 1,
    [Description("Generic failure.")]
    k_EResultFail = 2,
    [Description("Your Steam client doesn't have a connection to the back-end.")]
    k_EResultNoConnection = 3,
    [Description("Password/ticket is invalid.")]
    k_EResultInvalidPassword = 5,
    [Description("The user is logged in elsewhere.")]
    k_EResultLoggedInElsewhere = 6,
    [Description("Protocol version is incorrect.")]
    k_EResultInvalidProtocolVer = 7,
    [Description("A parameter is incorrect.")]
    k_EResultInvalidParam = 8,
    [Description("File was not found.")]
    k_EResultFileNotFound = 9,
    [Description("Called method is busy - action not taken.")]
    k_EResultBusy = 10,
    [Description("Called object was in an invalid state.")]
    k_EResultInvalidState = 11,
    [Description("The name was invalid.")]
    k_EResultInvalidName = 12,
    [Description("The email was invalid.")]
    k_EResultInvalidEmail = 13,
    [Description("The name is not unique.")]
    k_EResultDuplicateName = 14,
    [Description("Access is denied.")]
    k_EResultAccessDenied = 15,
    [Description("Operation timed out.")]
    k_EResultTimeout = 16,
    [Description("The user is VAC2 banned.")]
    k_EResultBanned = 17,
    [Description("Account not found.")]
    k_EResultAccountNotFound = 18,
    [Description("The Steam ID was invalid.")]
    k_EResultInvalidSteamID = 19,
    [Description("The requested service is currently unavailable.")]
    k_EResultServiceUnavailable = 20,
    [Description("The user is not logged on.")]
    k_EResultNotLoggedOn = 21,
    [Description("Request is pending, it may be in process or waiting on third party.")]
    k_EResultPending = 22,
    [Description("Encryption or Decryption failed.")]
    k_EResultEncryptionFailure = 23,
    [Description("Insufficient privilege.")]
    k_EResultInsufficientPrivilege = 24,
    [Description("Too much of a good thing.")]
    k_EResultLimitExceeded = 25,
    [Description("Access has been revoked.")]
    k_EResultRevoked = 26,
    [Description("License/Guest pass the user is trying to access is expired.")]
    k_EResultExpired = 27,
    [Description("Guest pass has already been redeemed by account, cannot be used again.")]
    k_EResultAlreadyRedeemed = 28,
    [Description("The request is a duplicate and the action has already occurred in the past, ignored this time.")]
    k_EResultDuplicateRequest = 29,
    [Description("All the games in this guest pass redemption request are already owned by the user.")]
    k_EResultAlreadyOwned = 30,
    [Description("IP address not found.")]
    k_EResultIPNotFound = 31,
    [Description("Failed to write change to the data store.")]
    k_EResultPersistFailed = 32,
    [Description("Failed to acquire access lock for this operation.")]
    k_EResultLockingFailed = 33,
    [Description("The logon session has been replaced.")]
    k_EResultLogonSessionReplaced = 34,
    [Description("Failed to connect.")]
    k_EResultConnectFailed = 35,
    [Description("The authentication handshake has failed.")]
    k_EResultHandshakeFailed = 36,
    [Description("There has been a generic IO failure.")]
    k_EResultIOFailure = 37,
    [Description("The remote server has disconnected.")]
    k_EResultRemoteDisconnect = 38,
    [Description("Failed to find the shopping cart requested.")]
    k_EResultShoppingCartNotFound = 39,
    [Description("A user blocked the action.")]
    k_EResultBlocked = 40,
    [Description("The target is ignoring sender.")]
    k_EResultIgnored = 41,
    [Description("Nothing matching the request found.")]
    k_EResultNoMatch = 42,
    [Description("The account is disabled.")]
    k_EResultAccountDisabled = 43,
    [Description("This service is not accepting content changes right now.")]
    k_EResultServiceReadOnly = 44,
    [Description("Account doesn't have value, so this feature isn't available.")]
    k_EResultAccountNotFeatured = 45,
    [Description("Allowed to take this action, but only because requester is admin.")]
    k_EResultAdministratorOK = 46,
    [Description("A Version mismatch in content transmitted within the Steam protocol.")]
    k_EResultContentVersion = 47,
    [Description("The current CM can't service the user making a request, user should try another.")]
    k_EResultTryAnotherCM = 48,
    [Description("You are already logged in elsewhere, this cached credential login has failed.")]
    k_EResultPasswordRequiredToKickSession = 49,
    [Description("The user is logged in elsewhere.")]
    k_EResultAlreadyLoggedInElsewhere = 50,
    [Description("Long running operation has suspended/paused (eg. content download).")]
    k_EResultSuspended = 51,
    [Description("Operation has been canceled, typically by user (eg. a content download).")]
    k_EResultCancelled = 52,
    [Description("Operation canceled because data is ill formed or unrecoverable.")]
    k_EResultDataCorruption = 53,
    [Description("Operation canceled - not enough disk space.")]
    k_EResultDiskFull = 54,
    [Description("The remote or IPC call has failed.")]
    k_EResultRemoteCallFailed = 55,
    [Description("Password could not be verified as it's unset server side.")]
    k_EResultPasswordUnset = 56,
    [Description("External account (PSN, Facebook...) is not linked to a Steam account.")]
    k_EResultExternalAccountUnlinked = 57,
    [Description("PSN ticket was invalid.")]
    k_EResultPSNTicketInvalid = 58,
    [Description("External account (PSN, Facebook...) is already linked to some other account, must explicitly request to replace/delete the link first.")]
    k_EResultExternalAccountAlreadyLinked = 59,
    [Description("The sync cannot resume due to a conflict between the local and remote files.")]
    k_EResultRemoteFileConflict = 60,
    [Description("The requested new password is not allowed.")]
    k_EResultIllegalPassword = 61,
    [Description("New value is the same as the old one. This is used for secret question and answer.")]
    k_EResultSameAsPreviousValue = 62,
    [Description("Account login denied due to 2nd factor authentication failure.")]
    k_EResultAccountLogonDenied = 63,
    [Description("The requested new password is not legal.")]
    k_EResultCannotUseOldPassword = 64,
    [Description("Account login denied due to auth code invalid.")]
    k_EResultInvalidLoginAuthCode = 65,
    [Description("Account login denied due to 2nd factor auth failure - and no mail has been sent.")]
    k_EResultAccountLogonDeniedNoMail = 66,
    [Description("The users hardware does not support Intel's Identity Protection Technology (IPT).")]
    k_EResultHardwareNotCapableOfIPT = 67,
    [Description("Intel's Identity Protection Technology (IPT) has failed to initialize.")]
    k_EResultIPTInitError = 68,
    [Description("Operation failed due to parental control restrictions for current user.")]
    k_EResultParentalControlRestricted = 69,
    [Description("Facebook query returned an error.")]
    k_EResultFacebookQueryError = 70,
    [Description("Account login denied due to an expired auth code.")]
    k_EResultExpiredLoginAuthCode = 71,
    [Description("The login failed due to an IP restriction.")]
    k_EResultIPLoginRestrictionFailed = 72,
    [Description("The current users account is currently locked for use. This is likely due to a hijacking and pending ownership verification.")]
    k_EResultAccountLockedDown = 73,
    [Description("The logon failed because the accounts email is not verified.")]
    k_EResultAccountLogonDeniedVerifiedEmailRequired = 74,
    [Description("There is no URL matching the provided values.")]
    k_EResultNoMatchingURL = 75,
    [Description("Bad Response due to a Parse failure, missing field, etc.")]
    k_EResultBadResponse = 76,
    [Description("The user cannot complete the action until they re-enter their password.")]
    k_EResultRequirePasswordReEntry = 77,
    [Description("The value entered is outside the acceptable range.")]
    k_EResultValueOutOfRange = 78,
    [Description("Something happened that we didn't expect to ever happen.")]
    k_EResultUnexpectedError = 79,
    [Description("The requested service has been configured to be unavailable.")]
    k_EResultDisabled = 80,
    [Description("The files submitted to the CEG server are not valid.")]
    k_EResultInvalidCEGSubmission = 81,
    [Description("The device being used is not allowed to perform this action.")]
    k_EResultRestrictedDevice = 82,
    [Description("The action could not be complete because it is region restricted.")]
    k_EResultRegionLocked = 83,
    [Description($"Temporary rate limit exceeded, try again later, different from {nameof(k_EResultLimitExceeded)} which may be permanent.")]
    k_EResultRateLimitExceeded = 84,
    [Description("Need two-factor code to login.")]
    k_EResultAccountLoginDeniedNeedTwoFactor = 85,
    [Description("The thing we're trying to access has been deleted.")]
    k_EResultItemDeleted = 86,
    [Description("Login attempt failed, try to throttle response to possible attacker.")]
    k_EResultAccountLoginDeniedThrottle = 87,
    [Description("Two factor authentication (Steam Guard) code is incorrect.")]
    k_EResultTwoFactorCodeMismatch = 88,
    [Description("The activation code for two-factor authentication (Steam Guard) didn't match.")]
    k_EResultTwoFactorActivationCodeMismatch = 89,
    [Description("The current account has been associated with multiple partners.")]
    k_EResultAccountAssociatedToMultiplePartners = 90,
    [Description("The data has not been modified.")]
    k_EResultNotModified = 91,
    [Description("The account does not have a mobile device associated with it.")]
    k_EResultNoMobileDevice = 92,
    [Description("The time presented is out of range or tolerance.")]
    k_EResultTimeNotSynced = 93,
    [Description("SMS code failure - no match, none pending, etc.")]
    k_EResultSmsCodeFailed = 94,
    [Description("Too many accounts access this resource.")]
    k_EResultAccountLimitExceeded = 95,
    [Description("Too many changes to this account.")]
    k_EResultAccountActivityLimitExceeded = 96,
    [Description("Too many changes to this phone.")]
    k_EResultPhoneActivityLimitExceeded = 97,
    [Description("Cannot refund to payment method, must use wallet.")]
    k_EResultRefundToWallet = 98,
    [Description("Cannot send an email.")]
    k_EResultEmailSendFailure = 99,
    [Description("Can't perform operation until payment has settled.")]
    k_EResultNotSettled = 100,
    [Description("The user needs to provide a valid captcha.")]
    k_EResultNeedCaptcha = 101,
    [Description("A game server login token owned by this token's owner has been banned.")]
    k_EResultGSLTDenied = 102,
    [Description("Game server owner is denied for some other reason such as account locked, community ban, vac ban, missing phone, etc.")]
    k_EResultGSOwnerDenied = 103,
    [Description("The type of thing we were requested to act on is invalid.")]
    k_EResultInvalidItemType = 104,
    [Description("The IP address has been banned from taking this action.")]
    k_EResultIPBanned = 105,
    [Description("This Game Server Login Token (GSLT) has expired from disuse; it can be reset for use.")]
    k_EResultGSLTExpired = 106,
    [Description("user doesn't have enough wallet funds to complete the action")]
    k_EResultInsufficientFunds = 107,
    [Description("There are too many of this thing pending already")]
    k_EResultTooManyPending = 108
}
