import React, { FC, useContext, useState as reactUseState } from 'react';
import { Layout, Menu } from 'antd';
import {
    MenuUnfoldOutlined,
    MenuFoldOutlined
} from '@ant-design/icons';
import { createState, State, useState as hookstateUseState } from '@hookstate/core';
import { Link } from 'react-router-dom';
const { Header, Sider, Content } = Layout;

export interface HeaderConfig {
    title: string
}

const headerConfig: State<HeaderConfig> = createState({ title: "(Not set)" });

export const setHeader = (title: string) => {
    setTimeout(() => {
        headerConfig.title.set(title)
    }, 10)
}

const CustomHeader: FC<{ title: string}> = ({ title }) => {
    return (
        <div style={{ display: 'inline-block' }}>
            <div style={{ display: 'flex', flexDirection: 'column' }}>
                <h1 style={{ display: 'inline-block', lineHeight: '15px' }}>{title}</h1>
                <small style={{ display: 'inline-block', lineHeight: '15px'}}>TODO: Breadcrumb</small>
            </div>
        </div>
    )
}

const Shell: FC<{ logo: any, menu: React.ReactElement }> = ({ logo, menu, children, ...props }) => {
    const [collapsed, setCollapsed] = reactUseState(false);
    const headerState = hookstateUseState(headerConfig);
    return (
        <Layout>
            {menu && <Sider trigger={null} collapsible collapsed={collapsed}>
                <div className="logo">
                    <Link to="/">
                        {logo && <img src={logo} alt="SPT AB" height={18} />}
                    </Link>
                </div>
                {menu}
            </Sider>}
            <Layout className="site-layout">
                <Header className="site-layout-background" style={{ padding: 0 }}>
                    {React.createElement(collapsed ? MenuUnfoldOutlined : MenuFoldOutlined, {
                        className: 'trigger',
                        onClick: () => setCollapsed(!collapsed),
                    })}
                    <CustomHeader title={headerState.title.value} />
                </Header>
                <Content
                    className="site-layout-background"
                    style={{
                        margin: '24px 16px',
                        padding: 24,
                        minHeight: 280,
                    }}>
                    { children }
                </Content>
            </Layout>
        </Layout>
    )
}

export default Shell;