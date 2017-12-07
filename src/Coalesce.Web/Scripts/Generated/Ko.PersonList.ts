
/// <reference path="../coalesce.dependencies.d.ts" />

// Knockout List View Model for: Person
// Auto Generated by IntelliTect.Coalesce

var baseUrl = baseUrl || '';

module ListViewModels {

    export interface DataSource<T extends Coalesce.BaseViewModel<T>> { }

    // Add an enum for all methods that are static and IQueryable
    module PersonDataSources {
        export class Default implements DataSource<ViewModels.Person> { }
        export class NamesStartingWithAWithCases implements DataSource<ViewModels.Person> { }
        export class BorCPeople implements DataSource<ViewModels.Person> { }
    }

    export class PersonList extends Coalesce.BaseListViewModel<PersonList, ViewModels.Person> {
        protected modelName = "Person";

        protected apiController = "/Person";

        public modelKeyName = "personId";
        public dataSources = PersonDataSources;
        public itemClass = ViewModels.Person;

        public query: {
            where?: string;
            personId?:number;
            title?:number;
            firstName?:string;
            lastName?:string;
            email?:string;
            gender?:number;
            name?:string;
            companyId?:number;
        } = null;

        // The custom code to run in order to pull the initial datasource to use for the collection that should be returned
        public dataSource: DataSource<ViewModels.Person> = new PersonDataSources.Default();

        public static coalesceConfig = new Coalesce.ListViewModelConfiguration<PersonList, ViewModels.Person>(Coalesce.GlobalConfiguration.listViewModel);
        public coalesceConfig = new Coalesce.ListViewModelConfiguration<PersonList, ViewModels.Person>(PersonList.coalesceConfig);


        // Call server method (Add)
        // Adds two numbers.
        public add = (numberOne: number, numberTwo: number, callback: () => void = null, reload: boolean = true) => {
            var source = new this.dataSources.BorCPeople();

            this.addIsLoading(true);
            this.addMessage('');
            this.addWasSuccessful(null);
            $.ajax({ method: "POST",
                     url: this.coalesceConfig.baseApiUrl() + "/Person/Add",
                     data: { numberOne: numberOne, numberTwo: numberTwo },
                     xhrFields: { withCredentials: true } })
            .done((data) => {
                this.addResultRaw(data.object);
                this.addMessage('');
                this.addWasSuccessful(true);
                this.addResult(data.object);
        
                if (reload) {
                    this.load(callback);
                } else if ($.isFunction(callback)) {
                    callback();
                }
            })
            .fail((xhr) => {
                var errorMsg = "Unknown Error";
                if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                this.addWasSuccessful(false);
                this.addMessage(errorMsg);

                //alert("Could not call method add: " + errorMsg);
            })
            .always(() => {
                this.addIsLoading(false);
            });
        } 
        // Result of server method (Add) strongly typed in a observable.
        public addResult: KnockoutObservable<number> = ko.observable(null);
        // Raw result object of server method (Add) simply wrapped in an observable.
        public addResultRaw: KnockoutObservable<any> = ko.observable();
        // True while the server method (Add) is being called
        public addIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (Add) if it fails.
        public addMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (Add) was successful.
        public addWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (Add)
        public addUi = (callback: () => void = null) => {
            var numberOne: number = parseFloat(prompt('Number One'));
            var numberTwo: number = parseFloat(prompt('Number Two'));
            this.add(numberOne, numberTwo, callback);
        }
        // Presents a modal with input boxes to call the server method (Add)
        public addModal = (callback: () => void = null) => {
            $('#method-Add').modal();
            $('#method-Add').on('shown.bs.modal', () => {
                $('#method-Add .btn-ok').unbind('click');
                $('#method-Add .btn-ok').click(() => {
                    this.addWithArgs(null, callback);
                    $('#method-Add').modal('hide');
                });
            });
        }
            // Variable for method arguments to allow for easy binding
        public addWithArgs = (args?: PersonList.AddArgs, callback: () => void = null) => {
            if (!args) args = this.addArgs;
            this.add(args.numberOne(), args.numberTwo(), callback);
        }
        public addArgs = new PersonList.AddArgs(); 
        

        // Call server method (GetUser)
        // Returns the user name
        public getUser = (callback: () => void = null, reload: boolean = true) => {
            this.getUserIsLoading(true);
            this.getUserMessage('');
            this.getUserWasSuccessful(null);
            $.ajax({ method: "POST",
                     url: this.coalesceConfig.baseApiUrl() + "/Person/GetUser",
                     data: {  },
                     xhrFields: { withCredentials: true } })
            .done((data) => {
                this.getUserResultRaw(data.object);
                this.getUserMessage('');
                this.getUserWasSuccessful(true);
                this.getUserResult(data.object);
        
                if (reload) {
                    this.load(callback);
                } else if ($.isFunction(callback)) {
                    callback();
                }
            })
            .fail((xhr) => {
                var errorMsg = "Unknown Error";
                if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                this.getUserWasSuccessful(false);
                this.getUserMessage(errorMsg);

                //alert("Could not call method getUser: " + errorMsg);
            })
            .always(() => {
                this.getUserIsLoading(false);
            });
        } 
        // Result of server method (GetUser) strongly typed in a observable.
        public getUserResult: KnockoutObservable<string> = ko.observable(null);
        // Raw result object of server method (GetUser) simply wrapped in an observable.
        public getUserResultRaw: KnockoutObservable<any> = ko.observable();
        // True while the server method (GetUser) is being called
        public getUserIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (GetUser) if it fails.
        public getUserMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (GetUser) was successful.
        public getUserWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (GetUser)
        public getUserUi = (callback: () => void = null) => {
            this.getUser(callback);
        }
        // Presents a modal with input boxes to call the server method (GetUser)
        public getUserModal = (callback: () => void = null) => {
            this.getUserUi(callback);
        }
        

        // Call server method (GetUserPublic)
        // Returns the user name
        public getUserPublic = (callback: () => void = null, reload: boolean = true) => {
            this.getUserPublicIsLoading(true);
            this.getUserPublicMessage('');
            this.getUserPublicWasSuccessful(null);
            $.ajax({ method: "POST",
                     url: this.coalesceConfig.baseApiUrl() + "/Person/GetUserPublic",
                     data: {  },
                     xhrFields: { withCredentials: true } })
            .done((data) => {
                this.getUserPublicResultRaw(data.object);
                this.getUserPublicMessage('');
                this.getUserPublicWasSuccessful(true);
                this.getUserPublicResult(data.object);
        
                if (reload) {
                    this.load(callback);
                } else if ($.isFunction(callback)) {
                    callback();
                }
            })
            .fail((xhr) => {
                var errorMsg = "Unknown Error";
                if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                this.getUserPublicWasSuccessful(false);
                this.getUserPublicMessage(errorMsg);

                //alert("Could not call method getUserPublic: " + errorMsg);
            })
            .always(() => {
                this.getUserPublicIsLoading(false);
            });
        } 
        // Result of server method (GetUserPublic) strongly typed in a observable.
        public getUserPublicResult: KnockoutObservable<string> = ko.observable(null);
        // Raw result object of server method (GetUserPublic) simply wrapped in an observable.
        public getUserPublicResultRaw: KnockoutObservable<any> = ko.observable();
        // True while the server method (GetUserPublic) is being called
        public getUserPublicIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (GetUserPublic) if it fails.
        public getUserPublicMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (GetUserPublic) was successful.
        public getUserPublicWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (GetUserPublic)
        public getUserPublicUi = (callback: () => void = null) => {
            this.getUserPublic(callback);
        }
        // Presents a modal with input boxes to call the server method (GetUserPublic)
        public getUserPublicModal = (callback: () => void = null) => {
            this.getUserPublicUi(callback);
        }
        

        // Call server method (NamesStartingWith)
        // Gets all the first names starting with the characters.
        public namesStartingWith = (characters: string, callback: () => void = null, reload: boolean = true) => {
            this.namesStartingWithIsLoading(true);
            this.namesStartingWithMessage('');
            this.namesStartingWithWasSuccessful(null);
            $.ajax({ method: "POST",
                     url: this.coalesceConfig.baseApiUrl() + "/Person/NamesStartingWith",
                     data: { characters: characters },
                     xhrFields: { withCredentials: true } })
            .done((data) => {
                this.namesStartingWithResultRaw(data.object);
                this.namesStartingWithMessage('');
                this.namesStartingWithWasSuccessful(true);
                this.namesStartingWithResult(data.object);
        
                if (reload) {
                    this.load(callback);
                } else if ($.isFunction(callback)) {
                    callback();
                }
            })
            .fail((xhr) => {
                var errorMsg = "Unknown Error";
                if (xhr.responseJSON && xhr.responseJSON.message) errorMsg = xhr.responseJSON.message;
                this.namesStartingWithWasSuccessful(false);
                this.namesStartingWithMessage(errorMsg);

                //alert("Could not call method namesStartingWith: " + errorMsg);
            })
            .always(() => {
                this.namesStartingWithIsLoading(false);
            });
        } 
        // Result of server method (NamesStartingWith) strongly typed in a observable.
        public namesStartingWithResult: KnockoutObservableArray<string> = ko.observableArray([]);
        // Raw result object of server method (NamesStartingWith) simply wrapped in an observable.
        public namesStartingWithResultRaw: KnockoutObservable<any> = ko.observable();
        // True while the server method (NamesStartingWith) is being called
        public namesStartingWithIsLoading: KnockoutObservable<boolean> = ko.observable(false);
        // Error message for server method (NamesStartingWith) if it fails.
        public namesStartingWithMessage: KnockoutObservable<string> = ko.observable(null);
        // True if the server method (NamesStartingWith) was successful.
        public namesStartingWithWasSuccessful: KnockoutObservable<boolean> = ko.observable(null);
        // Presents a series of input boxes to call the server method (NamesStartingWith)
        public namesStartingWithUi = (callback: () => void = null) => {
            var characters: string = prompt('Characters');
            this.namesStartingWith(characters, callback);
        }
        // Presents a modal with input boxes to call the server method (NamesStartingWith)
        public namesStartingWithModal = (callback: () => void = null) => {
            $('#method-NamesStartingWith').modal();
            $('#method-NamesStartingWith').on('shown.bs.modal', () => {
                $('#method-NamesStartingWith .btn-ok').unbind('click');
                $('#method-NamesStartingWith .btn-ok').click(() => {
                    this.namesStartingWithWithArgs(null, callback);
                    $('#method-NamesStartingWith').modal('hide');
                });
            });
        }
            // Variable for method arguments to allow for easy binding
        public namesStartingWithWithArgs = (args?: PersonList.NamesStartingWithArgs, callback: () => void = null) => {
            if (!args) args = this.namesStartingWithArgs;
            this.namesStartingWith(args.characters(), callback);
        }
        public namesStartingWithArgs = new PersonList.NamesStartingWithArgs(); 
        

        protected createItem = (newItem?: any, parent?: any) => new ViewModels.Person(newItem, parent);

        constructor() {
            super();
        }
    }

    export namespace PersonList {
        // Classes for use in method calls to support data binding for input for arguments
        export class AddArgs {
            public numberOne: KnockoutObservable<number> = ko.observable(null);
            public numberTwo: KnockoutObservable<number> = ko.observable(null);
        }
        export class NamesStartingWithArgs {
            public characters: KnockoutObservable<string> = ko.observable(null);
        }
    }
}