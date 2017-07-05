
/// <reference path="../coalesce.dependencies.d.ts" />

// Knockout List View Model for: Company
// Auto Generated by IntelliTect.Coalesce

var baseUrl = baseUrl || '';

module ListViewModels {

    // Add an enum for all methods that are static and IQueryable
    export enum CompanyDataSources {
            Default,
        }
    export class CompanyList extends Coalesce.BaseListViewModel<CompanyList, ViewModels.Company> {
        protected modelName = "Company";

        protected apiController = "/Company";

        public modelKeyName = "companyId";
        public dataSources = CompanyDataSources;
        public itemClass = ViewModels.Company;

        public query: {
            where?: string;
            companyId?:number;
            name?:String;
            address1?:String;
            address2?:String;
            city?:String;
            state?:String;
            zipCode?:String;
            altName?:String;
        } = null;

        // The custom code to run in order to pull the initial datasource to use for the collection that should be returned
        public listDataSource: CompanyDataSources = CompanyDataSources.Default;

        public static coalesceConfig = new Coalesce.ListViewModelConfiguration<CompanyList, ViewModels.Company>(Coalesce.GlobalConfiguration.listViewModel);
        public coalesceConfig = new Coalesce.ListViewModelConfiguration<CompanyList, ViewModels.Company>(CompanyList.coalesceConfig);

        // Valid values
    
        protected createItem = (newItem?: any, parent?: any) => new ViewModels.Company(newItem, parent);

        constructor() {
            super();
        }
    }

    export namespace CompanyList {
        // Classes for use in method calls to support data binding for input for arguments
    }
}