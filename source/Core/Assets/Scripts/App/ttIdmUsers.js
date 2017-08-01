/// <reference path="../Libs/angular.min.js" />
/// <reference path="../Libs/angular-route.min.js" />

(function (angular) {

    var app = angular.module("ttIdmUsers", ['ngRoute', 'ttIdm', 'ttIdmUI']);
    function config($routeProvider, PathBase) {
        $routeProvider
            .when("/users/list/:filter?/:page?", {
                controller: 'ListUsersCtrl',
                resolve: { users: "idmUsers" },
                templateUrl: PathBase + '/assets/Templates.users.list.html'
            })
            .when("/users/create", {
                controller: 'NewUserCtrl',
                resolve: {
                    api: function (idmApi) {
                        return idmApi.get();
                    }
                },
                templateUrl: PathBase + '/assets/Templates.users.new.html'
            })
            .when("/users/edit/:subject", {
                controller: 'EditUserCtrl',
                resolve: { users: "idmUsers" },
                templateUrl: PathBase + '/assets/Templates.users.edit.html'
            });
    }
    config.$inject = ["$routeProvider", "PathBase"];
    app.config(config);

    function ListUsersCtrl($scope, idmUsers, idmPager, $routeParams, $location) {
        var model = {
            message : null,
            users : null,
            pager : null,
            waiting : true,
            filter : $routeParams.filter,
            page : $routeParams.page || 1
        };
        $scope.model = model;

        $scope.search = function (filter) {
            var url = "/users/list";
            if (filter) {
                url += "/" + filter;
            }
            $location.url(url);
        };

        var itemsPerPage = 10;
        var startItem = (model.page - 1) * itemsPerPage;

        idmUsers.getUsers(model.filter, startItem, itemsPerPage).then(function (result) {
            $scope.model.waiting = false;
            $scope.model.users = result.data.items;
            if (result.data.items && result.data.items.length) {
                $scope.model.pager = new idmPager(result.data, itemsPerPage);
            }
        }, function (error) {
            $scope.model.message = error;
            $scope.model.waiting = false;
        });
    }
    ListUsersCtrl.$inject = ["$scope", "idmUsers", "idmPager", "$routeParams", "$location"];
    app.controller("ListUsersCtrl", ListUsersCtrl);

    function NewUserCtrl($scope, idmUsers, api, ttFeedback) {
        var feedback = new ttFeedback();
        $scope.feedback = feedback;
        if (!api.links.createUser) {
            feedback.errors = "Create Not Supported";
            return;
        }
        else {
            var properties = api.links.createUser.meta
                .map(function (item) {
                    return {
                        meta : item,
                        data : item.dataType === 5 ? false : undefined
                    };
                });
            $scope.properties = properties;
            $scope.create = function (properties) {
                var props = properties.map(function (item) {
                    return {
                        type: item.meta.type,
                        value: item.data
                    };
                });
                idmUsers.createUser(props)
                    .then(function (result) {
                        $scope.last = result;
                        feedback.message = "Create Success";
                    }, feedback.errorHandler);
            };
        }
    }
    NewUserCtrl.$inject = ["$scope", "idmUsers", "api", "ttFeedback"];
    app.controller("NewUserCtrl", NewUserCtrl);

    function EditUserCtrl($scope, idmUsers, $routeParams, ttFeedback) {
        var feedback = new ttFeedback();
        $scope.feedback = feedback;

        function loadUser() {
            return idmUsers.getUser($routeParams.subject)
                .then(function (result) {
                    $scope.user = result;

                    if (!result.data.properties) {
                        $scope.tab = 1;

                        if (!result.data.roles) {
                            $scope.tab = 2;
                        }
                    }

                }, feedback.errorHandler);
        };
        loadUser();

        $scope.setProperty = function (property) {
            idmUsers.setProperty(property)
                .then(function () {
                    if (property.meta.dataType !== 1) {
                        feedback.message = property.meta.name + " Changed to: " + property.data;
                    }
                    else {
                        feedback.message = property.meta.name + " Changed";
                    }
                    loadUser();
                }, feedback.errorHandler);
        };

        $scope.addClaim = function (claims, claim) {
            idmUsers.addClaim(claims, claim)
                .then(function () {
                    feedback.message = "Claim Added : " + claim.type + ", " + claim.value;
                    loadUser();
                }, feedback.errorHandler);
        };

        $scope.removeClaim = function (claim) {
            idmUsers.removeClaim(claim)
                .then(function () {
                    feedback.message = "Claim Removed : " + claim.data.type + ", " + claim.data.value;
                    loadUser().then(function () {
                        $scope.claim = claim.data;
                    });
                }, feedback.errorHandler);
        };

        $scope.addExternalLogin = function (externalLogins, externalLogin) {
            idmUsers.addExternalLogin(externalLogins, externalLogin)
                .then(function () {
                    feedback.message = "External Login Added : " + externalLogin.provider + ", " + externalLogin.providerId;
                    loadUser();
                }, feedback.errorHandler);
        };

        $scope.removeExternalLogin = function (externalLogin) {
            idmUsers.removeExternalLogin(externalLogin)
                .then(function () {
                    feedback.message = "External Login Removed : " + externalLogin.data.provider + ", " + externalLogin.data.providerId;
                    loadUser().then(function () {
                        $scope.externalLogin = claim.externalLogin;
                    });
                }, feedback.errorHandler);
        };

        $scope.deleteUser = function (user) {
            idmUsers.deleteUser(user)
                .then(function () {
                    feedback.message = "User Deleted";
                    $scope.user = null;
                }, feedback.errorHandler);
        };

        $scope.setRole = function (role) {
            if (role.data) {
                idmUsers.addRole(role)
                    .then(function () {
                        feedback.message = "Role Added : " + role.meta.type;
                        loadUser();
                    }, feedback.errorHandler);
            }
            else {
                idmUsers.removeRole(role)
                    .then(function () {
                        feedback.message = "Role Removed : " + role.meta.type;
                        loadUser();
                    }, feedback.errorHandler);
            }
        };
    }
    EditUserCtrl.$inject = ["$scope", "idmUsers", "$routeParams", "ttFeedback"];
    app.controller("EditUserCtrl", EditUserCtrl);

})(angular);
