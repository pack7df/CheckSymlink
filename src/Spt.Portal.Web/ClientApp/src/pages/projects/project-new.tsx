import Form from 'antd/lib/form';
import React, { FC } from 'react';
import { WithTranslation, withTranslation } from 'react-i18next';
import { CommandResult } from 'src/core/stores/types';
import { NewProjectItem, ProjectItemStore } from 'src/stores/projects-store';
import { container } from 'src/inversify.config';
import Modal from 'antd/lib/modal/Modal';
import { Alert, Input, Spin } from 'antd';
import { nameof } from 'src/core/utils/object';

const NewProjectDialog: FC<{ onClose: (response?: CommandResult<NewProjectItem>) => void } & WithTranslation> = ({ t, onClose }) => {
    const [form] = Form.useForm();

    const newProjectStore = container.get(ProjectItemStore)
    const newProjectState = newProjectStore.state;

    const onNewItem = async () => {
        let item = await form.validateFields() as NewProjectItem;
        let response = await newProjectStore.create(item);
        if (response) {
            form.resetFields();
            onClose(response);
        }
    };

    return <Modal
        title={t("New project")}
        visible={true}
        onOk={onNewItem}
        onCancel={()=>onClose()}>
        {newProjectState.errorMessage.get() && <Alert type='error' message={t(newProjectState.errorMessage.value || "")} />}
        <Spin spinning={newProjectState.isBusy.get()}>
            <Form form={form} layout="vertical" autoComplete="off">
                <Form.Item name={nameof<NewProjectItem>("name")} label={t("name")} rules={[{ required: true, message: t("Please enter a project name") }]}>
                    <Input autoComplete="off"  />
                </Form.Item>
            </Form>
        </Spin>
    </Modal>
}

export default withTranslation()(NewProjectDialog)