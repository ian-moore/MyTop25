import React from 'react';

class Header extends React.Component {
    render() {
        return (
            <div className="header">
                <h1>{this.props.name}'s Top 25 Artists</h1>
            </div>
        )
    }
}

export default Header;