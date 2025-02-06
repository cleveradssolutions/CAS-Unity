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

/// Type representing a Unity AdView ad client.
typedef const void * CASViewClientRef;
/// Type representing a CASUView type
typedef const void * CASUViewRef;

/// Type representing a NSObject<CASStatusHandler> type
typedef const void * CASImpressionRef;

// MARK: - CAS Mediation Manager callbacks
typedef void (*CASUInitializationCompleteCallback)(CASManagerClientRef *manager,
                                                   const char          *error,
                                                   const char          *countryCode,
                                                   BOOL                isConsentRequired,
                                                   BOOL                isTestMode);

typedef void (*CASUActionCallback)(CASManagerClientRef *manager,
                                   int                 action,
                                   int                 type,
                                   int                 error,
                                   const char          *errorMessage);

typedef void (*CASUImpressionCallback)(CASManagerClientRef *manager, int action, int type, CASImpressionRef impression);


// MARK: - CAS AdView callbacks
typedef void (*CASUViewActionCallback)(CASViewClientRef *view,
                                       int action,
                                       int error,
                                       const char          *errorMessage);
typedef void (*CASUViewImpressionCallback)(CASViewClientRef *view, CASImpressionRef impression);
typedef void (*CASUViewRectCallback)(CASViewClientRef *view, float x, float y, float width, float height);

typedef void (*CASUConsentFlowCompletion)(int status);

// #c# consts AdActionCode
#define kCASUAction_LOADED          1
#define kCASUAction_FAILED          2
#define kCASUAction_SHOWN           3
#define kCASUAction_IMPRESSION      4
#define kCASUAction_SHOW_FAILED     5
#define kCASUAction_CLICKED         6
#define kCASUAction_COMPLETED       7
#define kCASUAction_CLOSED          8
#define kCASUAction_VIEW_RECT       9
#define kCASUAction_INIT            10

// C# consts AdTypeCode
#define kCASUType_BANNER            0
#define kCASUType_INTER             1
#define kCASUType_REWARD            2
#define kCASUType_APP_OPEN          3
#define kCASUType_APP_RETURN        5
#define kCASUType_MANAGER           6

// C# Enum AdPosition
#define kCASUPosition_TOP_CENTER    0
#define kCASUPosition_TOP_LEFT      1
#define kCASUPosition_TOP_RIGHT     2
#define kCASUPosition_BOTTOM_CENTER 3
#define kCASUPosition_BOTTOM_LEFT   4
#define kCASUPosition_BOTTOM_RIGHT  5
#define kCASUPosition_MIDDLE_CENTER 6
#define kCASUPosition_MIDDLE_LEFT   7
#define kCASUPosition_MIDDLE_RIGHT  8

// C# Enum AdSize
#define kCASUSize_BANNER            1
#define kCASUSize_ADAPTIVE          2
#define kCASUSize_SMART             3
#define kCASUSize_LEADER            4
#define kCASUSize_MREC              5
#define kCASUSize_FULL_WIDTH        6
#define kCASUSize_LINE              7

#define kCASULOGTAG @"[CAS.AI-UB] "
#define kCASUMETHOD_NOT_SUPPORT @"Unity bridge not support method with AdType index: "

#endif // ifndef CASUTypes_h
