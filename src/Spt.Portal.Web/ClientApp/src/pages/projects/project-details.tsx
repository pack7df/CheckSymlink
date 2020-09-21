import { Card } from 'antd';
import React, { FC, useEffect, useRef } from 'react';
import { withTranslation, WithTranslation } from 'react-i18next';
import { RouteComponentProps } from 'react-router';
import { setHeader } from 'src/core/ui/shell';
import { ProjectItemStore } from 'src/stores/projects-store';
import { container } from 'src/inversify.config';

const ProjectDetailPage: FC<{} & RouteComponentProps<{ id: string }> & WithTranslation> = ({ t, match }) => {
    const inputRef = useRef();
    
    let projectId = match.params.id;
    const projectStore = container.get(ProjectItemStore);
    const projectState = projectStore.state;

    useEffect(
        () => {
            projectStore.load(projectId);
        },
        [inputRef]
    );

    if (projectState.item.value)
        setHeader(t("Project {0}").replace("{0}", projectState.item.name.value));

    return (
        <Card>
            TODO: Detalle del proyecto
        </Card>
    )
}

export default withTranslation()(ProjectDetailPage)