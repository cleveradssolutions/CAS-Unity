//
//  CSAUTypes.h
//  CASUnityPlugin
//
//  Copyright © 2020 Clever Ads Solutions. All rights reserved.
//

typedef const void * CASUTypeManagerClientRef;
typedef const void * CASUTypeManagerRef;
typedef const void * CASUTypeManagerBuilderRef;

typedef const void * CASUTypeViewClientRef;
typedef const void * CASUTypeViewRef;

typedef void (*CASUInitializationCompleteCallback)(CASUTypeManagerClientRef *manager, const char *error, BOOL withConsent);

typedef void (*CASUDidLoadedAdCallback)(CASUTypeManagerClientRef *manager);

typedef void (*CASUDidFailedAdCallback)(CASUTypeManagerClientRef *manager, NSInteger error);

typedef void (*CASUWillOpeningWithMetaCallback)(CASUTypeManagerClientRef *manager, const char *parameters);

typedef void (*CASUDidShowAdFailedWithErrorCallback)(CASUTypeManagerClientRef *manager, const char *error);

typedef void (*CASUDidClickedAdCallback)(CASUTypeManagerClientRef *manager);

typedef void (*CASUDidCompletedAdCallback)(CASUTypeManagerClientRef *manager);

typedef void (*CASUDidClosedAdCallback)(CASUTypeManagerClientRef *manager);

typedef void (*CASUATTCompletion)(NSUInteger status);
