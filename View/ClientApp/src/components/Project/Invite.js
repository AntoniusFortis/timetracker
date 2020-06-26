import React, { PureComponent } from 'react';
import { Post } from '../../restManager';
import cogoToast from 'cogo-toast';

export class ProjectInvite extends PureComponent {
    constructor(props) {
        super(props);

        this.state = {
            user_input: ""
        };
    }

    onUserInputChange = (e) => {
        this.setState({ user_input: e.target.value });
    }

    onSubmit = (event) => {
        event.preventDefault();

        const { user_input } = this.state;

        const projectId = this.props.match.params.projectId;
        const body = { ProjectId: projectId, UserName: user_input };

        Post("api/project/addUserToProject", body, (response) => {
            response.json().then(result => {
                if (result.status === 200) {
                    window.location.href = "/project/get/" + projectId;
                }
                else {
                    cogoToast.error(result.message);
                }
            });
        });
    }

    render() {
        return (
            <div style={{ width: '300px', margin: '0 auto', paddingTop: '125px', height: '300px', display: 'block' }}>
                <form style={{ width: '400px' }} onSubmit={this.onSubmit}>
                    <input required style={{ width: '270px', textAlign: 'center' }} maxLength="20" min="3" type="text" placeholder="Логин пользователя" onChange={this.onUserInputChange} />
                    <input style={{ display: 'block', width: '270px', marginTop: '15px' }} type="submit" value="Добавить" />
                </form>
            </div>
        );
    }
}
