/// <reference path="../Libs/angular.min.js" />
/// <reference path="../Libs/oidc-token-manager.min.js" />

(function (angular) {
    var app = angular.module("ttIdm", []);

    function config($httpProvider) {
        function intercept($q, idmTokenManager, idmErrorService) {
            return {
                'request': function (config) {
                    idmErrorService.clear();
                    var token = idmTokenManager.access_token;
                    if (token) {
                        config.headers['Authorization'] = 'Bearer ' + token;
                    }
                    return config;
                },
                'responseError': function (response) {
                    if (response.status === 401) {
                        //idmTokenManager.removeToken();
                    }
                    if (response.status === 403) {
                        //idmTokenManager.removeToken();
                    }
                    return $q.reject(response);
                }
            };
        };
        intercept.$inject = ["$q", "idmTokenManager", "idmErrorService"];
        $httpProvider.interceptors.push(intercept);
    };
    config.$inject = ["$httpProvider"];
    app.config(config);

    function idmErrorService($rootScope, $timeout) {
        var svc = {
            show: function (err) {
                $timeout(function () {
                    if (err instanceof Array) {
                        $rootScope.errors = err;
                    }
                    else {
                        $rootScope.errors = [err];
                    }
                }, 100);
            },
            clear: function () {
                $rootScope.errors = null;
            }
        };

        return svc;
    }
    idmErrorService.$inject = ["$rootScope", "$timeout"];
    app.factory("idmErrorService", idmErrorService);

    function idmTokenManager(OidcTokenManager, oauthSettings, PathBase, $window, $rootScope) {

        oauthSettings.response_type = "token";

        var mgr = new OidcTokenManager(oauthSettings);

        var applyFuncs = [
                "_callTokenRemoved", "_callTokenExpiring",
                "_callTokenExpired", "_callTokenObtained",
                "_callSilentTokenRenewFailed"
        ];
        applyFuncs.forEach(function (name) {
            var tmp = mgr[name].bind(mgr);
            mgr[name] = function () {
                $rootScope.$applyAsync(function () {
                    tmp();
                });
            }
        });

        return mgr;
    }
    idmTokenManager.$inject = ["OidcTokenManager", "oauthSettings", "PathBase", "$window", "$rootScope"];
    app.factory("idmTokenManager", idmTokenManager);

    function idmApi(idmTokenManager, $http, $q, PathBase) {
        var cache = null;

        idmTokenManager.addOnTokenRemoved(function () {
            cache = null;
        });

        return {
            get: function () {
                if (cache) {
                    var d = $q.defer();
                    d.resolve(cache);
                    return d.promise;
                }

                return $http.get(PathBase + "/api").then(function (resp) {
                    cache = resp.data;
                    return cache;
                }, function (resp) {
                    cache = null;
                    if (resp.status === 401) {
                        throw 'You are not authorized to use this service.';
                    }
                    else {
                        throw (resp.data && (resp.data.exceptionMessage || resp.data.message)) || 'Failed to access IdentityManager API.';
                    }
                });
            }
        };
    }
    idmApi.$inject = ["idmTokenManager", "$http", "$q", "PathBase"];
    app.factory("idmApi", idmApi);

    function idmUsers($http, idmApi, $log) {
        function nop() {
        }
        function mapResponseData(response) {
            return response.data;
        }
        function errorHandler(msg) {
            msg = msg || "Unexpected Error";
            return function (response) {
                if (response.data.exceptionMessage) {
                    $log.error(response.data.exceptionMessage);
                }
                throw (response.data.errors || response.data.message || msg);
            }
        }

        var svc = idmApi.get().then(function (api) {
            svc.getUsers = function (filter, start, count) {
                return $http.get(api.links.users, { params: { filter: filter, start: start, count: count } })
                    .then(mapResponseData, errorHandler("Error Getting Users"));
            };

            svc.getUser = function (subject) {
                return $http.get(api.links.users + "/" + encodeURIComponent(subject))
                    .then(mapResponseData, errorHandler("Error Getting User"));
            };

            if (api.links.createUser) {
                svc.createUser = function (properties) {
                    return $http.post(api.links.createUser.href, properties)
                        .then(mapResponseData, errorHandler("Error Creating User"));
                };
            }

            svc.deleteUser = function (user) {
                return $http.delete(user.links.delete)
                    .then(nop, errorHandler("Error Deleting User"));
            };

            svc.setProperty = function (property) {
                if (property.data === 0) {
                    property.data = "0";
                }
                if (property.data === false) {
                    property.data = "false";
                }
                return $http.put(property.links.update, property.data)
                    .then(nop, errorHandler(property.meta && property.meta.name && "Error Setting " + property.meta.name || "Error Setting Property"));
            };

            svc.addClaim = function (claims, claim) {
                return $http.post(claims.links.create, claim)
                    .then(nop, errorHandler("Error Adding Claim"));
            };
            svc.removeClaim = function (claim) {
                return $http.delete(claim.links.delete)
                    .then(nop, errorHandler("Error Removing Claim"));
            };

            svc.addExternalLogin = function (externalLogins, externalLogin) {
                return $http.post(externalLogins.links.create, externalLogin)
                    .then(nop, errorHandler("Error Adding External Login"));
            };
            svc.removeExternalLogin = function (externalLogin) {
                return $http.delete(externalLogin.links.delete)
                    .then(nop, errorHandler("Error Removing External Login"));
            };

            svc.addRole = function (role) {
                return $http.post(role.links.add)
                    .then(nop, errorHandler("Error Adding Role"));
            };

            svc.removeRole = function (role) {
                return $http.delete(role.links.remove)
                    .then(nop, errorHandler("Error Removing Role"));
            };
        });

        return svc;
    }
    idmUsers.$inject = ["$http", "idmApi", "$log"];
    app.factory("idmUsers", idmUsers);

    function idmRoles($http, idmApi, $log) {
        function nop() {
        }
        function mapResponseData(response) {
            return response.data;
        }
        function errorHandler(msg) {
            msg = msg || "Unexpected Error";
            return function (response) {
                if (response.data.exceptionMessage) {
                    $log.error(response.data.exceptionMessage);
                }
                throw (response.data.errors || response.data.message || msg);
            }
        }

        var svc = idmApi.get().then(function (api) {
            svc.getRoles = function (filter, start, count) {
                return $http.get(api.links.roles, { params: { filter: filter, start: start, count: count } })
                    .then(mapResponseData, errorHandler("Error Getting Roles"));
            };

            svc.getRole = function (subject) {
                return $http.get(api.links.roles + "/" + encodeURIComponent(subject))
                    .then(mapResponseData, errorHandler("Error Getting Role"));
            };

            svc.setProperty = function (property) {
                if (property.data === 0) {
                    property.data = "0";
                }
                if (property.data === false) {
                    property.data = "false";
                }
                return $http.put(property.links.update, property.data)
                    .then(nop, errorHandler(property.meta && property.meta.name && "Error Setting " + property.meta.name || "Error Setting Property"));
            };

            if (api.links.createRole) {
                svc.createRole = function (properties) {
                    return $http.post(api.links.createRole.href, properties)
                        .then(mapResponseData, errorHandler("Error Creating Role"));
                };
            }

            svc.deleteRole = function (role) {
                return $http.delete(role.links.delete)
                    .then(nop, errorHandler("Error Deleting Role"));
            };
        });

        return svc;
    }
    idmRoles.$inject = ["$http", "idmApi", "$log"];
    app.factory("idmRoles", idmRoles);
})(angular);

(function (angular) {
    var model = document.getElementById("model").textContent.trim();
    model = JSON.parse(model);
    for (var key in model) {
        angular.module("ttIdm").constant(key, model[key]);
    }
    angular.module("ttIdm").constant("OidcTokenManager", OidcTokenManager);
})(angular);
