import React, { FC } from 'react';
import { WithTranslation, withTranslation } from 'react-i18next';
import { withRouter, RouteComponentProps } from 'react-router';
import { setHeader } from 'src/core/ui/shell';
import { Link } from 'react-router-dom';

const HomePage: FC<RouteComponentProps<any> & WithTranslation> = ({ t, ...props }) => {
    setHeader(t("Portal"))
    return (
        <div>
            <h1>{t("Welcome")}</h1>
            <p>TODO: Este es el portal del usuario</p>
            <ul>
                <li><Link to="/projects">{t("Projects")}</Link></li>
            </ul>
        </div>
    )
}

export default withTranslation()(withRouter(HomePage))