import React, { PureComponent } from 'react';
import { Post } from '../../restManager';

export class ProjectInvite extends PureComponent {
    constructor(props) {
        super(props);

        this.state = {
            user_input: ""
        };
    }

    onUserInputChange = (event) => {
        this.setState({ user_input: event.target.value });
    }

    onSubmit = (event) => {
        event.preventDefault();

        const { user_input } = this.state;

        const projectId = this.props.match.params.projectId;
        const body = { ProjectId: projectId, UserName: user_input };

        Post("api/project/addUserToProject", body, (response) => {
            if (response.status != 200) {
                console.log(response.statusText);
            }
            response.json().then(result => {
                    window.location.href = "/project/get/" + projectId;
            });
        });
    }

    render() {
        return (
            <form onSubmit={this.onSubmit}>
                <input type="text" placeholder="Имя пользователя" onChange={this.onUserInputChange} />
                <input type="submit" value="Принять" />
            </form>
        );
    }
}
