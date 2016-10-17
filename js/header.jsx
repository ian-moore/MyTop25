import React from 'react';

class Header extends React.Component {
    render() {
        return (
            <div className="header">
                <h1>{this.props.name}'s Top&nbsp;25&nbsp;Artists</h1>
            </div>
        )
    }
}

export default Header;