import DataStore from 'src/core/stores/data-store';
import FormStore from 'src/core/stores/form-store';

export interface ProjectSummary {
    id: string
    name: string
}


export interface ProjectItem {
    id: string
    name: string
}

export interface NewProjectItem {
    name: string
}

export class ProjectSummaryStore extends DataStore<ProjectSummary> {
    constructor(baseUrl: string) {
        super(`${baseUrl}/api/projects`, []);
    }
}

export class ProjectItemStore extends FormStore<ProjectItem, NewProjectItem> {
    constructor(baseUrl: string) {
        super(`${baseUrl}/api/projects`);
    }
}