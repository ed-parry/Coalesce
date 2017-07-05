
/// <reference path="../coalesce.dependencies.d.ts" />

// Knockout List View Model for: DevTeam
// Auto Generated by IntelliTect.Coalesce

var baseUrl = baseUrl || '';

module ListViewModels {

    // Add an enum for all methods that are static and IQueryable
    export enum DevTeamDataSources {
            Default,
        }
    export class DevTeamList extends Coalesce.BaseListViewModel<DevTeamList, ViewModels.DevTeam> {
        protected modelName = "DevTeam";

        protected apiController = "/DevTeam";

        public modelKeyName = "devTeamId";
        public dataSources = DevTeamDataSources;
        public itemClass = ViewModels.DevTeam;

        public query: {
            where?: string;
            devTeamId?:number;
            name?:String;
        } = null;

        // The custom code to run in order to pull the initial datasource to use for the collection that should be returned
        public listDataSource: DevTeamDataSources = DevTeamDataSources.Default;

        public static coalesceConfig = new Coalesce.ListViewModelConfiguration<DevTeamList, ViewModels.DevTeam>(Coalesce.GlobalConfiguration.listViewModel);
        public coalesceConfig = new Coalesce.ListViewModelConfiguration<DevTeamList, ViewModels.DevTeam>(DevTeamList.coalesceConfig);

        // Valid values
    
        protected createItem = (newItem?: any, parent?: any) => new ViewModels.DevTeam(newItem, parent);

        constructor() {
            super();
        }
    }

    export namespace DevTeamList {
        // Classes for use in method calls to support data binding for input for arguments
    }
}