import React, { FC } from 'react';
import { Route, withRouter, Switch, RouteComponentProps } from 'react-router';
import ProjectsListPage from './project-list';
import ProjectDetailsPage from './project-details';

const ProjectsIndex: FC<RouteComponentProps<any>> = ({ match }) => {
    return <Switch>
        <Route exact path={`${match.url}`} component={ProjectsListPage} />
        <Route exact path={`${match.url}/:id`} component={ProjectDetailsPage} />
    </Switch>
}

export default withRouter(ProjectsIndex)