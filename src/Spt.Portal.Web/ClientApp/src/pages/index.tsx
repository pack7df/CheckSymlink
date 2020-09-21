import React, { FC, useContext } from 'react';
import { Route, withRouter, Switch, RouteComponentProps } from 'react-router';
import HomePage from './home';
import ProjectsPage from './projects';
import Shell from 'src/core/ui/shell'
import { Menu } from 'antd';
import SecureContent from 'src/core/services/authentication';
import { withTranslation, WithTranslation } from 'react-i18next';
import { AppConfig } from 'src/inversify.config';
import {
    ProfileOutlined,
    VideoCameraOutlined,
    UploadOutlined
} from '@ant-design/icons';
import { Link } from 'react-router-dom';
const logo = require('src/core/assets/images/logo-spt-big.png');

const HomeIndex: FC<RouteComponentProps<any> & WithTranslation> = ({ match, t }) => {
    const config = useContext(AppConfig) as any;
    return <SecureContent
        config={{
            authority: config!.identityServerUrl,
            redirectUri: `${config!.serviceUrl}/portal`,
            resource: config!.scopes,
            clientId: config!.clientId,
            passthrough: config!.passthrough
        }}>
        <Shell
            logo={logo}
            menu={
                <Menu theme="dark" mode="inline" defaultSelectedKeys={[]}>
                    <Menu.Item key="users" icon={<ProfileOutlined />}>
                        <Link to={`${match.url}projects`}>{t("Projects")}</Link>
                    </Menu.Item>
                    <Menu.Item key="2" icon={<VideoCameraOutlined />}>
                        nav 2
                    </Menu.Item>
                    <Menu.Item key="3" icon={<UploadOutlined />}>
                        nav 3
                </Menu.Item>
                </Menu>
            }>
            <Switch>
                <Route exact path={`${match.url}`} component={HomePage} />
                <Route path={`${match.url}projects`} component={ProjectsPage} />
            </Switch>
        </Shell>
    </SecureContent>
}

export default withTranslation()(withRouter(HomeIndex))