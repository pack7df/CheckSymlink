import { ProjectOutlined } from '@ant-design/icons';
import React, { FC, useEffect, useRef, useState } from 'react';
import { WithTranslation, withTranslation } from 'react-i18next';
import { Route, withRouter, Switch, RouteComponentProps } from 'react-router';
import { Link } from 'react-router-dom';
import { setHeader } from 'src/core/ui/shell';
import { container } from 'src/inversify.config';
import { ProjectSummary, ProjectSummaryStore } from 'src/stores/projects-store';
import { TableModel, TableView } from 'src/core/ui/collections/table';
import { Card, Row, Tag } from 'antd';
import { Query } from 'src/core/stores/data-store';
import { formatMessage } from 'src/core/services/http.service';
import NewProjectDialog from './project-new';

const ProjectsListPage: FC<RouteComponentProps<any> & WithTranslation> = ({ match, t, history }) => {
    const [query, setQuery] = useState({ searchQuery: '', orderBy: [{ field: 'project', direction: 'Ascending', useProfile: false }], skip: 0, take: 10 } as Query)

    const inputRef = useRef();
    const projectsStore = container.get(ProjectSummaryStore)
    const projectsState = projectsStore.state;
    const [newProjectVisible, setNewProjectVisible] = useState(false)

    setHeader(t("Projects"))

    useEffect(
        () => {
            projectsStore.load({});
        },
        [inputRef]
    );

    const tableModel = {
        query: {},
        columns: [
            {
                sortable: true,
                searchable: true,
                field: "name",
                title: t("Name"),
                renderer: (data) => <span>
                    <Link to={`${match.url}/${data.id}`}><ProjectOutlined />&nbsp;<span>{data.name}</span></Link>
                </span>
            }
        ],
        data: projectsState.value,
        sortFields: [
            {
                field: "project",
                text: t("Project name"),
                useProfile: false
            }
        ]
    } as TableModel<ProjectSummary>;


    return (
        <Row align="top" justify="space-between">
            <Card style={{ width: "100%" }}>
                <TableView
                    rowKey="id"
                    canCreateNew
                    onNewItem={() => setNewProjectVisible(true)}
                    canDelete
                    onDeleteRow={(item: any) => projectsStore.delete(item.id)}
                    onQueryChanged={(query: Query) => { setQuery(query); projectsStore.load(query); }}
                    onRefresh={() => projectsStore.load(query)}
                    model={tableModel}
                    error={projectsState.errorMessage.value && formatMessage(projectsState.errorMessage.value)}
                />
                {newProjectVisible && <NewProjectDialog onClose={(response) => { setNewProjectVisible(false); if (response) { history.push(`${match.url}/${response.identity}`); } } } />}
            </Card>
        </Row>
    )
}

export default withTranslation()(withRouter(ProjectsListPage))