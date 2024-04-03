//
//  CSAUTypes.h
//  CASUnityPlugin
//
//  Copyright © 2024 CleverAdsSolutions LTD, CAS.AI. All rights reserved.
//

#ifndef CASUTypes_h
#define CASUTypes_h

/// Type representing a Unity MediationManager ad client.
typedef const void * CASManagerClientRef;
/// Type representing a CASMediationManager type
typedef const void * CASUManagerRef;
/// Type representing a CASManagerBuilder type
typedef const void * CASManagerBuilderRef;

/// Type representing a Unity AdView ad client.
typedef const void * CASViewClientRef;
/// Type representing a CASUView type
typedef const void * CASUViewRef;

/// Type representing a NSObject<CASStatusHandler> type
typedef const void * CASImpressionRef;

/// Type representing a CASConsentFlow
typedef const void * CASConsentFlowRef;

// MARK: - CAS Mediation Manager callbacks
typedef void (*CASUInitializationCompleteCallback)(CASManagerClientRef *manager, const char *error, const char *countryCode, BOOL withConsent, BOOL isTestMode);

typedef void (*CASUDidLoadedAdCallback)(CASManagerClientRef *manager, int type);
typedef void (*CASUDidFailedAdCallback)(CASManagerClientRef *manager, int type, int error);

typedef void (*CASUWillPresentAdCallback)(CASManagerClientRef *manager, int type, CASImpressionRef impression);
typedef void (*CASUDidShowAdFailedWithErrorCallback)(CASManagerClientRef *manager, int type, int error);
typedef void (*CASUDidClickedAdCallback)(CASManagerClientRef *manager, int type);
typedef void (*CASUDidCompletedAdCallback)(CASManagerClientRef *manager, int type);
typedef void (*CASUDidClosedAdCallback)(CASManagerClientRef *manager, int type);

// MARK: - CAS AdView callbacks
typedef void (*CASUViewDidLoadCallback)(CASViewClientRef *view);
typedef void (*CASUViewDidFailedCallback)(CASViewClientRef *view, NSInteger error);
typedef void (*CASUViewWillPresentCallback)(CASViewClientRef *view, CASImpressionRef impression);
typedef void (*CASUViewDidClickedCallback)(CASViewClientRef *view);
typedef void (*CASUViewDidRectCallback)(CASViewClientRef *view, float x, float y, float width, float height);

typedef void (*CASUConsentFlowCompletion)(NSUInteger status);

#define kCASUType_BANNER     0
#define kCASUType_INTER      1
#define kCASUType_REWARD     2
#define kCASUType_APP_OPEN   3
#define kCASUType_APP_RETURN 5
#define kCASUType_MANAGER    6

// C# Enum AdPosition
#define kCASUPosition_TOP_CENTER 0
#define kCASUPosition_TOP_LEFT 1
#define kCASUPosition_TOP_RIGHT 2
#define kCASUPosition_BOTTOM_CENTER 3
#define kCASUPosition_BOTTOM_LEFT 4
#define kCASUPosition_BOTTOM_RIGHT 5

// C# Enum AdSize
#define kCASUSize_BANNER 1
#define kCASUSize_ADAPTIVE 2
#define kCASUSize_SMART 3
#define kCASUSize_LEADER 4
#define kCASUSize_MREC 5
#define kCASUSize_FULL_WIDTH 6
#define kCASUSize_LINE 7

#endif // ifndef CASUTypes_h
