import React from 'react';

class Header extends React.Component {
    render() {
        return (
            <div className="header">
                <h2>{this.props.name}'s Top 25 Artists</h2>
            </div>
        )
    }
}

export default Header;