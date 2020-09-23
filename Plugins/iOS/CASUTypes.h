//
//  CSAUTypes.h
//  CASUnityPlugin
//
//  Copyright © 2020 Clever Ads Solutions. All rights reserved.
//

typedef const void * CASUTypeManagerClientRef;
/// Type representing a CASUManager.
typedef const void * CASUTypeManagerRef;


typedef void (*CASUInitializationCompleteCallback)(CASUTypeManagerClientRef *client,
                                                   BOOL                     success,
                                                   const char               *error);

typedef void (*CASUDidAdLoadedCallback)(CASUTypeManagerClientRef *client,
                                        NSInteger                adType);

typedef void (*CASUDidAdFailedToLoadCallback)(CASUTypeManagerClientRef *client,
                                              NSInteger                adType,
                                              const char               *error);

typedef void (*CASUWillShownWithAdCallback)(CASUTypeManagerClientRef *client);

typedef void (*CASUDidShowAdFailedWithErrorCallback)(CASUTypeManagerClientRef *client,
                                                     const char                *error);

typedef void (*CASUDidClickedAdCallback)(CASUTypeManagerClientRef *client);

typedef void (*CASUDidCompletedAdCallback)(CASUTypeManagerClientRef *client);

typedef void (*CASUDidClosedAdCallback)(CASUTypeManagerClientRef *client);
